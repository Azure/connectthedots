var debug       = require('debug')('amqp10-EndFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),

    constants   = require('./../constants'),
    u           = require('../utilities'),
    up          = u.payload,

    AMQPError   = require('../types/amqp_error'),
    DescribedType   = require('../types/described_type'),
    ForcedType  = require('../types/forced_type'),
    Symbol      = require('../types/symbol'),

    FrameBase   = require('./frame');

/**
 * <h2>end performative</h2>
 * <i>end the Session</i>
 * <p>Indicates that the Session has ended.</p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:end:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000017</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>error</td><td>error</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>error causing the end</i>
 * <p>
 *             If set, this field indicates that the Session is being ended due to an error condition.
 *             The value of the field should contain details on the cause of the error.
 *           </p></td></tr>
 * </table>
 *
 * @constructor
 */
function EndFrame(options) {
    EndFrame.super_.call(this);
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

util.inherits(EndFrame, FrameBase.AMQPFrame);

EndFrame.Descriptor = {
    name: new Symbol('amqp:end:list'),
    code: new Int64(0x00000000, 0x00000017)
};

EndFrame.prototype._getPerformative = function() {
    var values = [];
    if (this.error) {
        values.push(this.error);
    }
    return new DescribedType(EndFrame.Descriptor.code, values);
};

EndFrame.prototype.readPerformative = function(describedType) {
    var input = describedType.value;
    this.error = input[0];
};

module.exports = EndFrame;
