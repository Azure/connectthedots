var debug       = require('debug')('amqp10-FlowFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),

    constants   = require('../constants'),
    exceptions  = require('../exceptions'),
    u           = require('../utilities'),
    up          = u.payload,

    DescribedType   = require('../types/described_type'),
    ForcedType  = require('../types/forced_type'),
    Symbol      = require('../types/symbol'),

    FrameBase   = require('./frame');

/**
 * <h2>flow performative</h2>
 * <i>update link state</i>
 * <p>Updates the flow state for the specified Link.</p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:flow:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000013</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>next-incoming-id</td><td>transfer-number</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             Identifies the expected transfer-id of the next incoming  frame.
 *             This value is not set if and only if the sender has not yet received the
 *              frame for the session. See
 *              for more details.
 *           </p>
 * <p>transfer</p></td></tr>
 * <tr><td>incoming-window</td><td>uint</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             Defines the maximum number of incoming  frames that the endpoint
 *             can currently receive. See  for more
 *             details.
 *           </p>
 * <p>transfer</p></td></tr>
 * <tr><td>next-outgoing-id</td><td>transfer-number</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             The transfer-id that will be assigned to the next outgoing
 *             frame. See  for more details.
 *           </p>
 * <p>transfer</p></td></tr>
 * <tr><td>outgoing-window</td><td>uint</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             Defines the maximum number of outgoing  frames that the endpoint
 *             could potentially currently send, if it was not constrained by restrictions imposed by
 *             its peer's incoming-window. See  for more
 *             details.
 *           </p>
 * <p>transfer</p></td></tr>
 * <tr><td>handle</td><td>handle</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             If set, indicates that the flow frame carries flow state information for the local Link
 *             Endpoint associated with the given handle.  If not set, the flow frame is carrying only
 *             information pertaining to the Session Endpoint.
 *           </p>
 * <p>
 *             If set to a handle that is not currently associated with an attached Link, the
 *             recipient MUST respond by ending the session with an  session error.
 *           </p>
 * <p>session-error</p></td></tr>
 * <tr><td>delivery-count</td><td>sequence-no</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the endpoint's delivery-count</i>
 * <p>
 *             When the handle field is not set, this field MUST NOT be set.
 *           </p>
 * <p>
 *             When the handle identifies that the flow state is being sent from the Sender Link
 *             Endpoint to Receiver Link Endpoint this field MUST be set to the current delivery-count
 *             of the Link Endpoint.
 *           </p>
 * <p>
 *             When the flow state is being sent from the Receiver Endpoint to the Sender Endpoint this
 *             field MUST be set to the last known value of the corresponding Sending Endpoint. In the
 *             event that the Receiving Link Endpoint has not yet  seen the initial
 *              frame from the Sender this field MUST NOT be set.
 *           </p>
 * <p>attach</p>
 * <p>
 *             See  for more details.
 *           </p>
 * <p>flow-control</p></td></tr>
 * <tr><td>link-credit</td><td>uint</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the current maximum number of Messages that can be received</i>
 * <p>
 *             The current maximum number of Messages that can be handled at the Receiver
 *             Endpoint of the Link. Only the receiver endpoint can independently set this value. The
 *             sender endpoint sets this to the last known value seen from the receiver. See
 *              for more details.
 *           </p>
 * <p>flow-control</p>
 * <p>
 *             When the handle field is not set, this field MUST NOT be set.
 *           </p></td></tr>
 * <tr><td>available</td><td>uint</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the number of available Messages</i>
 * <p>
 *             The number of Messages awaiting credit at the link sender endpoint. Only the
 *             sender can independently set this value. The receiver sets this to the last known value
 *             seen from the sender. See  for more details.
 *           </p>
 * <p>flow-control</p>
 * <p>
 *             When the handle field is not set, this field MUST NOT be set.
 *           </p></td></tr>
 * <tr><td>drain</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates drain mode</i>
 * <p>
 *             When flow state is sent from the sender to the receiver, this field contains the actual
 *             drain mode of the sender. When flow state is sent from the receiver to the sender, this
 *             field contains the desired drain mode of the receiver. See  for more details.
 *           </p>
 * <p>flow-control</p>
 * <p>
 *             When the handle field is not set, this field MUST NOT be set.
 *           </p></td></tr>
 * <tr><td>echo</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>request link state from other endpoint</i></td></tr>
 * <tr><td>properties</td><td>fields</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>link state properties</i>
 * <p>
 *             A list of commonly defined link state properties and their meanings can be found here:
 *
 *           </p>
 * <p>http://www.amqp.org/specification/1.0/link-state-properties</p></td></tr>
 * </table>
 *
 * @constructor
 */
function FlowFrame(options) {
    FlowFrame.super_.call(this);
    this.channel = 0;
    if (options instanceof DescribedType) {
        this.readPerformative(options);
    } else {
        exceptions.assertArguments(options, ['incomingWindow', 'nextOutgoingId', 'outgoingWindow']);

        this.nextIncomingId = u.orNull(options.nextIncomingId);
        this.incomingWindow = options.incomingWindow;
        this.nextOutgoingId = options.nextOutgoingId;
        this.outgoingWindow = options.outgoingWindow;
        this.handle = u.orNull(options.handle);
        this.deliveryCount = u.orNull(options.deliveryCount);
        this.linkCredit = u.orNull(options.linkCredit);
        this.available = u.orNull(options.available);
        this.drain = u.orFalse(options.drain);
        this.echo = u.orFalse(options.echo);
        this.properties = u.onUndef(options.properties, {});
    }
}

util.inherits(FlowFrame, FrameBase.AMQPFrame);

FlowFrame.Descriptor = {
    name: new Symbol('amqp:flow:list'),
    code: new Int64(0x00000000, 0x00000013)
};

FlowFrame.prototype._getPerformative = function() {
    var self = this;
    return new DescribedType(FlowFrame.Descriptor.code, {
        nextIncomingId: new ForcedType('uint', self.nextIncomingId),
        incomingWindow: new ForcedType('uint', self.incomingWindow),
        nextOutgoingId: new ForcedType('uint', self.nextOutgoingId),
        outgoingWindow: new ForcedType('uint', self.outgoingWindow),
        handle: new ForcedType('uint', self.handle),
        deliveryCount: new ForcedType('uint', self.deliveryCount),
        linkCredit: new ForcedType('uint', self.linkCredit),
        available: new ForcedType('uint', self.available),
        drain: self.drain,
        echo: self.echo,
        properties: self.properties,
        encodeOrdering: [ 'nextIncomingId', 'incomingWindow', 'nextOutgoingId', 'outgoingWindow', 'handle',
            'deliveryCount', 'linkCredit', 'available', 'drain', 'echo', 'properties' ]
    });
};

FlowFrame.prototype.readPerformative = function(describedType) {
    up.assert(describedType, 1, 'incomingWindow');
    up.assert(describedType, 2, 'nextOutgoingId');
    up.assert(describedType, 3, 'outgoingWindow');

    var idx = 0;
    this.nextIncomingId = up.orNull(describedType, idx++);
    this.incomingWindow = up.get(describedType, idx++);
    this.nextOutgoingId = up.get(describedType, idx++);
    this.outgoingWindow = up.get(describedType, idx++);
    this.handle = up.orNull(describedType, idx++);
    this.deliveryCount = up.orNull(describedType, idx++);
    this.linkCredit = up.orNull(describedType, idx++);
    this.available = up.orNull(describedType, idx++);
    this.drain = up.orFalse(describedType, idx++);
    this.echo = up.orFalse(describedType, idx++);
    this.properties = up.onUndef(describedType, idx++, {});
};

module.exports = FlowFrame;
