var debug       = require('debug')('amqp10-CloseFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),

    constants   = require('../constants'),
    u           = require('../utilities'),
    up          = u.payload,

    AMQPError   = require('../types/amqp_error'),
    DescribedType   = require('../types/described_type'),
    Symbol      = require('../types/symbol'),

    FrameBase   = require('./frame');

/**
 * <h2>close performative</h2>
 * <i>signal a Connection close</i>
 * <p>
 *           Sending a close signals that the sender will not be sending any more frames (or bytes of
 *           any other kind) on the Connection. Orderly shutdown requires that this frame MUST be
 *           written by the sender. It is illegal to send any more frames (or bytes of any other kind)
 *           after sending a close frame.
 *         </p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:close:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000018</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>error</td><td>error</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>error causing the close</i>
 * <p>
 *             If set, this field indicates that the Connection is being closed due to an error
 *             condition. The value of the field should contain details on the cause of the error.
 *           </p></td></tr>
 * </table>
 *
 * @constructor
 */
function CloseFrame(options) {
    CloseFrame.super_.call(this);
    this.channel = 0;
    if (options) {
        if (options instanceof AMQPError) {
            this.error = options;
        } else if (options instanceof DescribedType) {
            this.readPerformative(options);
        } else {
            this.error = options.error;
        }
    }
}

util.inherits(CloseFrame, FrameBase.AMQPFrame);

CloseFrame.Descriptor = {
    name: new Symbol('amqp:close:list'),
    code: new Int64(0x00000000, 0x00000018)
};

CloseFrame.prototype._getPerformative = function() {
    var values = [];
    if (this.error) {
        values.push(this.error);
    }
    return new DescribedType(CloseFrame.Descriptor.code, values);
};

CloseFrame.prototype.readPerformative = function(describedType) {
    var input = describedType.value;
    this.error = input[0];
};

module.exports = CloseFrame;
