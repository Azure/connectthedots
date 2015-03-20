var debug       = require('debug')('amqp10-FrameReader'),

    Attach      = require('./attach_frame'),
    Begin       = require('./begin_frame'),
    Close       = require('./close_frame'),
    Detach      = require('./detach_frame'),
    Disposition = require('./disposition_frame'),
    End         = require('./end_frame'),
    Flow        = require('./flow_frame'),
    Open        = require('./open_frame'),
    Transfer    = require('./transfer_frame'),

    Sasl        = require('./sasl_frame'),

    codec       = require('../codec'),
    constants   = require('../constants'),
    exceptions  = require('../exceptions'),

    DescribedType   = require('../types/described_type'),
    M               = require('../types/message');

/**
 *
 * @constructor
 */
function FrameReader() {
}

/**
 * For now, just process performative headers.
 * @todo Need to process the payloads as well
 * @todo Cope with Non-AMQP frames
 *
 * @param cbuf          circular buffer containing the potential frame data.
 * @returns {AMQPFrame} Frame with populated data, undefined if frame is incomplete.  Throws exception on unmatched frame.
 */
FrameReader.prototype.read = function(cbuf) {
    if (cbuf.length < 8) return undefined;

    var size = cbuf.peek(4).readUInt32BE(0);
    if (size > cbuf.length) return undefined;

    var sizeAndDoff = cbuf.read(8);
    var doff = sizeAndDoff[4];
    var frameType = sizeAndDoff[5];
    var xHeaderSize = (doff * 4) - 8;
    var payloadSize = size - (doff * 4);
    if (xHeaderSize > 0) {
        xHeaderBuf = cbuf.read(xHeaderSize);
        // @todo Process x-header
        debug('Read extended header [' + xHeaderBuf.toString('hex') + ']');
    }

    var payloadBuf = null;
    if (payloadSize > 0) {
        payloadBuf = cbuf.read(payloadSize);
        if (frameType === constants.frameType.amqp) {
            var channel = sizeAndDoff.readUInt16BE(6); // Bytes 6 & 7 are channel
            var decoded = codec.decode(payloadBuf, 0);
            if (!decoded) throw new exceptions.MalformedPayloadError('Unable to parse frame payload [' + payloadBuf.toString('hex') + ']');
            if (!(decoded[0] instanceof DescribedType)) {
                throw new exceptions.MalformedPayloadError('Expected DescribedType from AMQP Payload, but received ' + JSON.stringify(decoded[0]));
            }
            var describedType = decoded[0];
            //debug('Rx on channel '+channel+': ' + JSON.stringify(describedType));
            switch (describedType.descriptor.toString()) {
                case Open.Descriptor.name.toString():
                case Open.Descriptor.code.toString():
                    return new Open(describedType);

                case Close.Descriptor.name.toString():
                case Close.Descriptor.code.toString():
                    return new Close(describedType);

                case Begin.Descriptor.name.toString():
                case Begin.Descriptor.code.toString():
                    var beginFrame = new Begin(describedType);
                    beginFrame.channel = channel;
                    return beginFrame;

                case End.Descriptor.name.toString():
                case End.Descriptor.code.toString():
                    var endFrame = new End(describedType);
                    endFrame.channel = channel;
                    return endFrame;

                case Attach.Descriptor.name.toString():
                case Attach.Descriptor.code.toString():
                    var attachFrame = new Attach(describedType);
                    attachFrame.channel = channel;
                    return attachFrame;

                case Detach.Descriptor.name.toString():
                case Detach.Descriptor.code.toString():
                    var detachFrame = new Detach(describedType);
                    detachFrame.channel = channel;
                    return detachFrame;

                case Flow.Descriptor.name.toString():
                case Flow.Descriptor.code.toString():
                    var flowFrame = new Flow(describedType);
                    flowFrame.channel = channel;
                    return flowFrame;

                case Transfer.Descriptor.name.toString():
                case Transfer.Descriptor.code.toString():
                    var transferFrame = new Transfer(describedType);
                    transferFrame.channel = channel;
                    transferFrame.message = this._readMessage(payloadBuf.slice(decoded[1]));
                    return transferFrame;

                case Disposition.Descriptor.name.toString():
                case Disposition.Descriptor.code.toString():
                    var dispoFrame = new Disposition(describedType);
                    dispoFrame.channel = channel;
                    return dispoFrame;

                default:
                    debug('Failed to match descriptor ' + describedType.descriptor.toString());
                    break;
            }
            throw new exceptions.MalformedPayloadError('Failed to match AMQP performative ' + describedType.descriptor.toString());
        } else if (frameType === constants.frameType.sasl) {
            var saslPayload = codec.decode(payloadBuf, 0);
            if (!(saslPayload[0] instanceof DescribedType)) {
                throw new exceptions.MalformedPayloadError('Expected DescribedType from AMQP Payload, but received ' + JSON.stringify(saslPayload[0]));
            }

            var saslType = saslPayload[0];
            debug('Rx SASL Frame: ' + JSON.stringify(saslType));
            switch (saslType.descriptor.toString()) {
                case Sasl.SaslInit.Descriptor.name.toString():
                case Sasl.SaslInit.Descriptor.code.toString():
                    return new Sasl.SaslInit(saslType);

                case Sasl.SaslMechanisms.Descriptor.name.toString():
                case Sasl.SaslMechanisms.Descriptor.code.toString():
                    return new Sasl.SaslMechanisms(saslType);

                case Sasl.SaslChallenge.Descriptor.name.toString():
                case Sasl.SaslChallenge.Descriptor.code.toString():
                    return new Sasl.SaslChallenge(saslType);

                case Sasl.SaslResponse.Descriptor.name.toString():
                case Sasl.SaslResponse.Descriptor.code.toString():
                    return new Sasl.SaslResponse(saslType);

                case Sasl.SaslOutcome.Descriptor.name.toString():
                case Sasl.SaslOutcome.Descriptor.code.toString():
                    return new Sasl.SaslOutcome(saslType);

                default:
                    debug('Failed to match SASL descriptor ' + saslType.descriptor.toString());
            }
            throw new exceptions.MalformedPayloadError('Failed to match SASL Frame ' + saslType.descriptor.toString());
        } else {
            throw new exceptions.NotImplementedError("We don't handle non-(AMQP|SASL) frames yet.");
        }

        throw new exceptions.MalformedPayloadError('Failed to match ' + (payloadBuf ? payloadBuf.toString('hex') : 'null'));
    } else {
        debug('Heartbeat frame: ' + sizeAndDoff.toString('hex'));
    }
};

/**
 * An AMQP Message is composed of:
 *
 * * Zero or one header
 * * Zero or one delivery-annotations
 * * Zero or one message-annotations
 * * Zero or one properties
 * * Zero or one application-properties
 * * Body: One or more data sections, one or more amqp-sequence sections, or one amqp-value section
 * * Zero or one footer
 *
 * @param {Buffer} messageBuf       Message buffer to decode
 * @returns {Message}               Complete message object decoded from buffer
 * @private
 */
FrameReader.prototype._readMessage = function(messageBuf) {
    var message = new M.Message();
    var body = [];
    var curIdx = 0;
    var possibleFields = {
        'header': M.Header, 'footer': M.Footer, 'deliveryAnnotations': M.DeliveryAnnotations,
        'annotations': M.Annotations, 'properties': M.Properties, 'applicationProperties': M.ApplicationProperties
    };
    var isData = function(x) { return x instanceof M.Data; };
    var isSequence = function(x) { return x instanceof M.AMQPSequence; };
    while (curIdx < messageBuf.length) {
        var decoded = codec.decode(messageBuf, curIdx);
        if (!decoded) throw new exceptions.MalformedPayloadError(
            'Unable to decode bytes from message body: ' + messageBuf.slice(curIdx).toString('hex'));
        curIdx += decoded[1];
        var matched = false;
        for (var fieldName in possibleFields) {
            if (decoded[0] instanceof possibleFields[fieldName]) {
                if (message[fieldName]) throw new exceptions.MalformedPayloadError('Duplicate '+fieldName+' section in message');
                message[fieldName] = decoded[0];
                matched = true;
                break;
            }
        }
        if (!matched) {
            // Part of the body
            if (decoded[0] instanceof M.Data) {
                if (body.length && !body.every(isData)) throw new exceptions.MalformedPayloadError(
                    'Attempt to put both Data and non-Data payloads in message body');
                body.push(decoded[0]);
            } else if (decoded[0] instanceof M.AMQPSequence) {
                if (body.length && !body.every(isSequence)) throw new exceptions.MalformedPayloadError(
                    'Attempt to put both AMQPSequence and non-AMQPSequence payloads in message body');
                body.push(decoded[0]);
            } else if (decoded[0] instanceof M.AMQPValue) {
                if (body.length) throw new exceptions.MalformedPayloadError('Attempt to provide more than one AMQPValue for message body');
                body.push(decoded[0]);
            } else {
                throw new exceptions.MalformedPayloadError('Unknown message contents: ' + JSON.stringify(decoded[0]));
            }
        }
    }
    // Pull out the values.
    message.body = body.map(function (x) { return x.getValue(); });

    return message;
};

module.exports = new FrameReader();
