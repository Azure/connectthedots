var debug           = require('debug')('amqp10-SaslFrame'),
    util            = require('util'),
    Int64           = require('node-int64'),

    codec           = require('./../codec'),
    constants       = require('../constants'),
    exceptions      = require('../exceptions'),
    u               = require('../utilities'),
    up              = u.payload,

    AMQPArray       = require('../types/amqp_composites').Array,
    DescribedType   = require('../types/described_type'),
    ForcedType      = require('../types/forced_type'),
    Symbol          = require('../types/symbol'),

    FrameBase       = require('./frame');

/**
 * Base Frame for SASL authentication.
 *
 * To establish a SASL tunnel, each peer MUST start by sending a protocol header. The protocol
 * header consists of the upper case ASCII letters "AMQP" followed by a protocol id of three,
 * followed by three unsigned bytes representing the major, minor, and revision of the
 * specification version (see constants.saslVersion). In total this is an 8-octet sequence:
 <pre>
 4 OCTETS   1 OCTET   1 OCTET   1 OCTET   1 OCTET
 +----------+---------+---------+---------+----------+
 |  "AMQP"  |   %d3   |  major  |  minor  | revision |
 +----------+---------+---------+---------+----------+
 </pre>
 *
 * Other than using a protocol id of three, the exchange of SASL tunnel headers follows the
 * same rules specified in the version negotiation section of the transport specification (See
 * version-negotiation).
 *
 * The following diagram illustrates the interaction involved in creating a SASL Security Layer:
 <pre>
 TCP Client                 TCP Server
 =========================================
 AMQP%d3.1.0.0  -------- >
 < --------  AMQP%d3.1.0.0
 :
 :
 &lt;SASL negotiation&gt;
 :
 :
 AMQP%d0.1.0.0  -------- >                (over SASL secured connection)
 < --------  AMQP%d0.1.0.0
 open  -------- >
 < --------  open
 </pre>
 *
 * SASL is negotiated using framing. A SASL frame has a type code of 0x01.
 * Bytes 6 and 7 of the header are ignored. Implementations SHOULD set these to 0x00. The
 * extended header is ignored. Implementations SHOULD therefore set DOFF to 0x02.
 *
 <pre>
 type: 0x01 - SASL frame
 +0       +1       +2       +3
 +-----------------------------------+ -.
 0 |                SIZE               |  |
 +-----------------------------------+  |-- > Frame Header
 4 |  DOFF  |  TYPE  |   &lt;IGNORED&gt;&#42;1   |  |      (8 bytes)
 +-----------------------------------+ -'
 +-----------------------------------+ -.
 8 |                ...                |  |
 .                                   .  |-- > Extended Header
 .             &lt;IGNORED&gt;&#42;2           .  |  (DOFF * 4 - 8) bytes
 |                ...                |  |
 +-----------------------------------+ -'
 +-----------------------------------+ -.
 4*DOFF |                                   |  |
 .    Sasl Mechanisms / Sasl Init    .  |
 .   Sasl Challenge / Sasl Response  .  |-- > Frame Body
 .           Sasl Outcome            .  |  (SIZE - DOFF * 4) bytes
 .                           ________|  |
 |                ...       |           |
 +--------------------------+          -'
 &#42;1 SHOULD be set to 0x0000
 &#42;2 Ignored, so DOFF should be set to 0x02
 </pre>
 *
 * The maximum size of a SASL frame is defined by constants.minMaxFrameSize. There is
 * no mechanism within the SASL negotiation to negotiate a different size. The frame body of a
 * SASL frame may contain exactly one AMQP type, whose type encoding must have
 * sasl-frame. Receipt of an empty frame is an irrecoverable error.
 *
 * @constructor
 */
function SaslFrame() {
    SaslFrame.super_.call(this, constants.frameType.sasl);
}

util.inherits(SaslFrame, FrameBase.Frame);
module.exports.SaslFrame = SaslFrame;

SaslFrame.prototype._getTypeSpecificHeader = function (options) {
    return 0;
};

SaslFrame.prototype._writeExtendedHeader = function (bufBuilder, options) {
    return 0; // SASL doesn't use the extended header.
};

SaslFrame.prototype._writePayload = function (bufBuilder, options) {
    throw new exceptions.NotImplementedError('Subclass must override _writePayload');
};

/**
 * A list of the sasl security mechanisms supported by the sending peer. It is invalid
 * for this list to be null or empty. If the sending peer does not require its partner
 * to authenticate with it, then it should send a list of one element with its value as
 * the SASL mechanism <i>ANONYMOUS</i>. The server mechanisms are ordered in decreasing
 * level of preference.
 *
 * @param options   Either the DescribedType of an incoming SASL Mechanisms frame,
 *                  or an array of mechanisms, or a map with a mechanisms key.
 * @constructor
 */
function SaslMechanisms(options) {
    SaslMechanisms.super_.call(this);
    if (options) {
        if (options instanceof DescribedType) {
            this.mechanisms = (options.value[0] instanceof Array) ?
                options.value[0].map(function (x) { return x.contents; }) :
                [ options.value[0].contents ];
        } else {
            this.mechanisms = options instanceof Array ?
                options : (options.mechanisms || []);
        }
    } else {
        this.mechanisms = [];
    }
}

util.inherits(SaslMechanisms, SaslFrame);
module.exports.SaslMechanisms = SaslMechanisms;

SaslMechanisms.Descriptor = {
    name: new Symbol('amqp:sasl-mechanisms:list'),
    code: new Int64(0x0, 0x40)
};

SaslMechanisms.prototype._writePayload = function(bufBuilder, options) {
    if (!this.mechanisms || this.mechanisms.length === 0) {
        this.mechanisms = [ 'ANONYMOUS' ];
    }

    var data = new DescribedType(SaslMechanisms.Descriptor.code);
    if (this.mechanisms.length === 1) {
        data.value = [ new Symbol(this.mechanisms[0]) ];
    } else {
        data.value = [ new AMQPArray(this.mechanisms, '0xA3') ];
    }

    codec.encode(data, bufBuilder);
};

/**
 * SASL Init frame, containing the following fields:
 * <table border="1">
 *     <tr><th>Name</th><th>Type</th><th>Mandatory</th><th>Multiple?</th></tr>
 *     <tr><td>mechanism</td><td>symbol</td><td>true</td><td>false</td></tr>
 *     <tr><td>&nbsp;</td><td colspan="3">
 * The name of the SASL mechanism used for the SASL exchange. If the selected mechanism is
 * not supported by the receiving peer, it MUST close the Connection with the
 * authentication-failure close-code. Each peer MUST authenticate using the highest-level
 * security profile it can handle from the list provided by the partner.
 *      </td></tr>
 *      <tr><td>initial-response</td><td>binary</td><td>false</td><td>false</td></tr>
 *      <tr><td>&nbsp;</td><td colspan="3">
 *      <i>security response data</i><br/>
 *      <p>
 * A block of opaque data passed to the security mechanism. The contents of this data are
 * defined by the SASL security mechanism.
 *      </p>
 *      </td></tr>
 *      <tr><td>hostname</td><td>string</td><td>false</td><td>false</td></tr>
 *      <tr><td>&nbsp;</td><td colspan="3">
 *      <i>the name of the target host</i><br/>
 *      <p>
 * The DNS name of the host (either fully qualified or relative) to which the sending peer
 * is connecting. It is not mandatory to provide the hostname. If no hostname is provided
 * the receiving peer should select a default based on its own configuration.
 *      </p>
 *      <p>
 * This field can be used by AMQP proxies to determine the correct back-end service to
 * connect the client to, and to determine the domain to validate the client's credentials
 * against.
 *      </p>
 *      <p>
 * This field may already have been specified by the server name indication extension as
 * described in RFC-4366, if a TLS layer is used, in which case this field SHOULD be null
 * or contain the same value. It is undefined what a different value to those already
 * specific means.
 *      </p>
 *      </td></tr>
 * </table>
 *
 * @param options
 * @constructor
 */
function SaslInit(options) {
    SaslInit.super_.call(this);
    if (options instanceof DescribedType) {
        this.mechanism = up.onUndef(options, 0, 'ANONYMOUS');
        if (this.mechanism instanceof Symbol) this.mechanism = this.mechanism.contents;
        this.initialResponse = up.get(options, 1);
        this.hostname = up.get(options, 2);
    } else {
        exceptions.assertArguments(options, ['mechanism']);
        this.mechanism = options.mechanism;
        this.initialResponse = options.initialResponse;
        this.hostname = options.hostname;
    }
}

util.inherits(SaslInit, SaslFrame);
module.exports.SaslInit = SaslInit;

SaslInit.Descriptor = {
    name: new Symbol('amqp:sasl-init:list'),
    code: new Int64(0x0, 0x41)
};

SaslInit.prototype._writePayload = function(bufBuilder, options) {
    var data = new DescribedType(SaslInit.Descriptor.code);
    data.value = [
        this.mechanism,
        u.orNull(this.initialResponse),
        u.orNull(this.hostname)
    ];
    codec.encode(data, bufBuilder);
};

/**
 * SASL Challenge frame, containing the following field:
 *
 * <table border="1">
 *     <tr><th>Name</th><th>Type</th><th>Mandatory</th><th>Multiple?</th></tr>
 *     <tr><td>challenge</td><td>binary</td><td>true</td><td>false</td></tr>
 *     <tr><td>&nbsp;</td><td colspan="3">
 * Challenge information, a block of opaque binary data passed to the security
 * mechanism.
 *     </td></tr>
 * </table>
 *
 * @param options
 * @constructor
 */
function SaslChallenge(options) {
    SaslChallenge.super_.call(this);
    if (options instanceof DescribedType) {
        this.challenge = up.orNull(options, 0);
    } else if (options instanceof Buffer) {
        this.challenge = options;
    } else {
        exceptions.assertArguments(options, ['challenge']);
        this.challenge = options.challenge;
    }
}

util.inherits(SaslChallenge, SaslFrame);
module.exports.SaslChallenge = SaslChallenge;

SaslChallenge.Descriptor = {
    name: new Symbol('amqp:sasl-challenge:list'),
    code: new Int64(0x0, 0x42)
};

SaslChallenge.prototype._writePayload = function(bufBuilder, options) {
    var data = new DescribedType(SaslChallenge.Descriptor.code);
    data.value = [
        this.challenge
    ];
    codec.encode(data, bufBuilder);
};

/**
 * SASL Response frame, containing the following field:
 *
 * <table border="1">
 *     <tr><th>Name</th><th>Type</th><th>Mandatory</th><th>Multiple?</th></tr>
 *     <tr><td>response</td><td>binary</td><td>true</td><td>false</td></tr>
 *     <tr><td>&nbsp;</td><td colspan="3">
 * A block of opaque data passed to the security mechanism. The contents of this data are
 * defined by the SASL security mechanism.
 *     </td></tr>
 * </table>
 *
 * @param options
 * @constructor
 */
function SaslResponse(options) {
    SaslResponse.super_.call(this);
    if (options instanceof DescribedType) {
        this.response = up.orNull(options, 0);
    } else if (options instanceof Buffer) {
        this.response = options;
    } else {
        exceptions.assertArguments(options, ['response']);
        this.response = options.response;
    }
}

util.inherits(SaslResponse, SaslFrame);
module.exports.SaslResponse = SaslResponse;

SaslResponse.Descriptor = {
    name: new Symbol('amqp:sasl-response:list'),
    code: new Int64(0x0, 0x43)
};

SaslResponse.prototype._writePayload = function(bufBuilder, options) {
    var data = new DescribedType(SaslResponse.Descriptor.code);
    data.value = [
        this.response
    ];
    codec.encode(data, bufBuilder);
};

/**
 * This frame indicates the outcome of the SASL dialog. Upon successful completion of the
 * SASL dialog the Security Layer has been established, and the peers must exchange protocol
 * headers to either start a nested Security Layer, or to establish the AMQP Connection.
 *
 * SASL Outcome frame contains the following fields:
 *
 * <table border="1">
 *     <tr><th>Name</th><th>Type</th><th>Mandatory</th><th>Multiple?</th></tr>
 *     <tr><td>code</td><td>sasl-code</td><td>true</td><td>false</td></tr>
 *     <tr><td>&nbsp;</td><td colspan="3">
 *     <i>indicates the outcome of the sasl dialog</i><br/>
 *     </td></tr>
 *     <tr><td>additional-data</td><td>binary</td><td>false</td><td>false</td></tr>
 *     <tr><td>&nbsp;</td><td colspan="3">
 * The additional-data field carries additional data on successful authentication outcome
 * as specified by the SASL specification (RFC-4422). If the authentication is
 * unsuccessful, this field is not set.
 *     </td></tr>
 * </table>
 *
 * SASL Code is a ubyte constrained to the following:
 * <table border="1">
 *     <tr><th>Byte</th><th>Name</th><th>Details</th></tr>
 *     <tr><td>0</td><td>ok</td><td>Connection authentication succeeded.</td></tr>
 *     <tr><td>1</td><td>auth</td><td>
 * Connection authentication failed due to an unspecified problem with the supplied
 * credentials.
 *     </td></tr>
 *     <tr><td>2</td><td>sys</td><td>
 * Connection authentication failed due to a system error.
 *     </td></tr>
 *     <tr><td>3</td><td>sys-perm</td><td>
 * Connection authentication failed due to a system error that is unlikely to be corrected
 * without intervention.
 *     </td></tr>
 *     <tr><td>4</td><td>sys-temp</td><td>
 * Connection authentication failed due to a transient system error.
 *     </td></tr>
 * </table>
 *
 * @param options
 * @constructor
 */
function SaslOutcome(options) {
    SaslOutcome.super_.call(this);
    if (options instanceof DescribedType) {
        up.assert(options, 0, 'code');
        this.code = up.get(options, 0);
        this.additionalData = up.get(options, 1);
        this.details = constants.saslOutcomes[this.code];
    } else {
        exceptions.assertArguments(options, ['code']);
        this.code = options.code;
        this.additionalData = options.additionalData;
    }
}

util.inherits(SaslOutcome, SaslFrame);
module.exports.SaslOutcome = SaslOutcome;

SaslOutcome.Descriptor = {
    name: new Symbol('amqp:sasl-outcome:list'),
    code: new Int64(0x0, 0x44)
};

SaslOutcome.prototype._writePayload = function(bufBuilder, options) {
    var data = new DescribedType(SaslOutcome.Descriptor.code);
    data.value = [
        new ForcedType('ubyte', this.code),
        u.orNull(this.additionalData)
    ];
    codec.encode(data, bufBuilder);
};
