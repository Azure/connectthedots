var debug       = require('debug')('amqp10-Sasl'),
    builder     = require('buffer-builder'),

    constants   = require('./constants'),
    codec       = require('./codec'),
    exceptions  = require('./exceptions'),
    u           = require('./utilities'),

    AMQPError   = require('./types/amqp_error'),
    DescribedType   = require('./types/described_type'),
    Symbol      = require('./types/symbol'),

    FrameReader = require('./frames/frame_reader'),
    SaslFrames  = require('./frames/sasl_frame'),

    Connection  = require('./connection');

/**
 * Currently, only supports SASL-PLAIN
 *
 * @constructor
 */
function Sasl() {
    this.receivedHeader = false;
}

Sasl.prototype.negotiate = function(connection, credentials, done) {
    exceptions.assertArguments(credentials, [ 'user', 'pass' ]);
    this.connection = connection;
    this.credentials = credentials;
    this.callback = done;
    var self = this;
    this._processFrameEH = function(frame) { self._processFrame(frame); };
    this.connection.on(Connection.FrameReceived, this._processFrameEH);
    this._sendHeader();
};

Sasl.prototype._sendHeader = function() {
    this.connection.sendHeader(constants.saslVersion);
};

Sasl.prototype.headerReceived = function(header) {
    debug('Server SASL Version: ' + header.toString('hex') + ' vs ' + constants.saslVersion.toString('hex'));
    if (u.bufferEquals(header, constants.saslVersion)) {
        this.receivedHeader = true;
        // Wait for mechanisms
    } else {
        this.callback(new exceptions.MalformedHeaderError('Invalid SASL Header ' + header.toString('hex')));
    }
};

Sasl.prototype._processFrame = function(frame) {
    if (frame instanceof SaslFrames.SaslMechanisms) {
        if (u.contains(frame.mechanisms, 'PLAIN')) {
            debug('Sending '+this.credentials.user+':'+this.credentials.pass);
            var buf = new builder();
            buf.appendUInt8(0); // <nul>
            buf.appendString(this.credentials.user);
            buf.appendUInt8(0); // <nul>
            buf.appendString(this.credentials.pass);
            var initFrame = new SaslFrames.SaslInit({
                mechanism: new Symbol('PLAIN'),
                initialResponse: buf.get()
            });
            this.connection.sendFrame(initFrame);
        } else {
            throw new exceptions.NotImplementedError('Only supports SASL-PLAIN at the moment.');
        }
    } else if (frame instanceof SaslFrames.SaslChallenge) {
        var responseFrame = new SaslFrames.SaslResponse({});
        this.connection.sendFrame(responseFrame);
    } else if (frame instanceof SaslFrames.SaslOutcome) {
        if (frame.code === constants.saslOutcomes.ok) {
            this.callback();
        } else {
            this.callback(new exceptions.AuthenticationError('SASL Failed: ' + frame.code + ': ' + frame.details));
        }
    }
};

// @todo Methods for sending init, receiving challenge, sending response, receiving outcome.

module.exports = Sasl;
