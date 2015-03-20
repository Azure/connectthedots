var debug       = require('debug')('amqp10-AttachFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),

    constants   = require('../constants'),
    exceptions  = require('../exceptions'),
    u           = require('../utilities'),
    up          = u.payload,

    DescribedType   = require('../types/described_type'),
    ForcedType  = require('../types/forced_type'),
    Symbol      = require('../types/symbol'),
    Source      = require('../types/source_target').Source,
    Target      = require('../types/source_target').Target,

    FrameBase   = require('./frame');

/**
 * <h2>attach performative</h2>
 * <i>attach a Link to a Session</i>
 * <p>
 *           The  frame indicates that a Link Endpoint has been
 *           attached to the Session. The opening flag is used to indicate that the Link Endpoint is
 *           newly created.
 *         </p>
 * <p>attach</p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:attach:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000012</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>name</td><td>string</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the name of the link</i>
 * <p>
 *             This name uniquely identifies the link from the container of the source to the container
 *             of the target node, e.g. if the container of the source node is A, and the container of
 *             the target node is B, the link may be globally identified by the (ordered) tuple
 *             .
 *           </p></td></tr>
 * <tr><td>handle</td><td>handle</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             The handle MUST NOT be used for other open Links. An attempt to attach using a handle
 *             which is already associated with a Link MUST be responded to with an immediate
 *              carrying a Handle-in-use .
 *            </p>
 * <p>close</p>
 * <p>
 *              To make it easier to monitor AMQP link attach frames, it is recommended that
 *              implementations always assign the lowest available handle to this field.
 *            </p></td></tr>
 * <tr><td>role</td><td>role</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>role of the link endpoint</i></td></tr>
 * <tr><td>snd-settle-mode</td><td>sender-settle-mode</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>settlement mode for the Sender</i>
 * <p>
 *             Determines the settlement policy for deliveries sent at the Sender. When set at the
 *             Receiver this indicates the desired value for the settlement mode at the Sender.  When
 *             set at the Sender this indicates the actual settlement mode in use.
 *           </p></td></tr>
 * <tr><td>rcv-settle-mode</td><td>receiver-settle-mode</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the settlement mode of the Receiver</i>
 * <p>
 *             Determines the settlement policy for unsettled deliveries received at the Receiver. When
 *             set at the Sender this indicates the desired value for the settlement mode at the
 *             Receiver. When set at the Receiver this indicates the actual settlement mode in use.
 *           </p></td></tr>
 * <tr><td>source</td><td>*</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the source for Messages</i>
 * <p>
 *             If no source is specified on an outgoing Link, then there is no source currently
 *             attached to the Link. A Link with no source will never produce outgoing Messages.
 *           </p></td></tr>
 * <tr><td>target</td><td>*</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the target for Messages</i>
 * <p>
 *             If no target is specified on an incoming Link, then there is no target currently
 *             attached to the Link. A Link with no target will never permit incoming Messages.
 *           </p></td></tr>
 * <tr><td>unsettled</td><td>map</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>unsettled delivery state</i>
 * <p>
 *             This is used to indicate any unsettled delivery states when a suspended link is resumed.
 *             The map is keyed by delivery-tag with values indicating the delivery state. The local
 *             and remote delivery states for a given delivery-tag MUST be compared to resolve any
 *             in-doubt deliveries. If necessary, deliveries MAY be resent, or resumed based on the
 *             outcome of this comparison. See .
 *           </p>
 * <p>resuming-deliveries</p>
 * <p>
 *             If the local unsettled map is too large to be encoded within a frame of the agreed
 *             maximum frame size then the session may be ended with the frame-size-too-small error
 *             (see ). The endpoint SHOULD make use of the ability to send an
 *             incomplete unsettled map (see below) to avoid sending an error.
 *           </p>
 * <p>amqp-error</p>
 * <p>
 *             The unsettled map MUST NOT contain null valued keys.
 *           </p>
 * <p>
 *             When reattaching (as opposed to resuming), the unsettled map MUST be null.
 *           </p></td></tr>
 * <tr><td>incomplete-unsettled</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             If set to true this field indicates that the unsettled map provided is not complete.
 *             When the map is incomplete the recipient of the map cannot take the absence of a
 *             delivery tag from the map as evidence of settlement. On receipt of an incomplete
 *             unsettled map a sending endpoint MUST NOT send any new deliveries (i.e. deliveries where
 *             resume is not set to true) to its partner (and a receiving endpoint which sent an
 *             incomplete unsettled map MUST detach with an error on receiving a transfer which does
 *             not have the resume flag set to true).
 *           </p></td></tr>
 * <tr><td>initial-delivery-count</td><td>sequence-no</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             This MUST NOT be null if role is sender, and it is ignored if the role is receiver. See
 *             .
 *           </p>
 * <p>flow-control</p></td></tr>
 * <tr><td>max-message-size</td><td>ulong</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the maximum message size supported by the link endpoint</i>
 * <p>
 *             This field indicates the maximum message size supported by the link endpoint. Any
 *             attempt to deliver a message larger than this results in a message-size-exceeded
 *             . If this field is zero or unset, there is no maximum size
 *             imposed by the link endpoint.
 *           </p>
 * <p>link-error</p></td></tr>
 * <tr><td>offered-capabilities</td><td>symbol</td><td>false</td><td>true</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the extension capabilities the sender supports</i>
 * <p>
 *             A list of commonly defined session capabilities and their meanings can be found here:
 *             .
 *           </p>
 * <p>http://www.amqp.org/specification/1.0/link-capabilities</p></td></tr>
 * <tr><td>desired-capabilities</td><td>symbol</td><td>false</td><td>true</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the extension capabilities the sender may use if the receiver supports them</i></td></tr>
 * <tr><td>properties</td><td>fields</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>link properties</i>
 * <p>
 *             The properties map contains a set of fields intended to indicate information about the
 *             link and its container.
 *           </p>
 * <p>
 *             A list of commonly defined link properties and their meanings can be found here:
 *
 *           </p>
 * <p>http://www.amqp.org/specification/1.0/link-properties</p></td></tr>
 * </table>
 *
 * @constructor
 */
function AttachFrame(options) {
    AttachFrame.super_.call(this);
    this.channel = 0;
    if (options instanceof DescribedType) {
        this.readPerformative(options);
    } else {
        exceptions.assertArguments(options, ['name', 'handle', 'role']);
        if (!options.source && !options.target) throw new exceptions.ArgumentError('source|target');
        if (options.role === constants.linkRole.sender &&
            options.initialDeliveryCount === undefined) throw new exceptions.ArgumentError('initialDeliveryCount for sender');

        this.name = options.name;
        this.handle = options.handle;
        this.role = options.role;
        this.senderSettleMode = u.onUndef(options.senderSettleMode, constants.senderSettleMode.mixed);
        this.receiverSettleMode = u.onUndef(options.receiverSettleMode, constants.receiverSettleMode.autoSettle);
        this.source = u.orNull(options.source);
        this.target = u.orNull(options.target);
        this.unsettled = u.onUndef(options.unsettled, {});
        this.incompleteUnsettled = u.orFalse(options.incompleteUnsettled);
        this.initialDeliveryCount = u.orNull(options.initialDeliveryCount);
        this.maxMessageSize = u.onUndef(options.maxMessageSize, 0);
        this.offeredCapabilities = u.orNull(options.offeredCapabilities);
        this.desiredCapabilities = u.orNull(options.desiredCapabilities);
        this.properties = u.onUndef(options.properties, {});
    }
}

util.inherits(AttachFrame, FrameBase.AMQPFrame);

AttachFrame.Descriptor = {
    name: new Symbol('amqp:attach:list'),
    code: new Int64(0x00000000, 0x00000012)
};

AttachFrame.prototype._getPerformative = function() {
    var self = this;
    var performative = new DescribedType(AttachFrame.Descriptor.code, {
        name: self.name,
        handle: new ForcedType('uint', self.handle),
        role: self.role,
        senderSettleMode: new ForcedType('ubyte', self.senderSettleMode),
        receiverSettleMode: new ForcedType('ubyte', self.receiverSettleMode),
        source: u.coerce(self.source, Source),
        target: u.coerce(self.target, Target),
        unsettled: self.unsettled,
        incompleteUnsettled: self.incompleteUnsettled,
        initialDeliveryCount: new ForcedType('uint', self.initialDeliveryCount),
        maxMessageSize: new ForcedType('ulong', self.maxMessageSize),
        offeredCapabilities: self.offeredCapabilities,
        desiredCapabilities: self.desiredCapabilities,
        properties: self.properties,
        encodeOrdering: [ 'name', 'handle', 'role', 'senderSettleMode', 'receiverSettleMode', 'source', 'target', 'unsettled',
            'incompleteUnsettled', 'initialDeliveryCount', 'maxMessageSize', 'offeredCapabilities', 'desiredCapabilities', 'properties']
    });
    return performative;
};

AttachFrame.prototype.readPerformative = function(describedType) {
    var idx = 0;
    this.name = up.get(describedType, idx++);
    this.handle = up.get(describedType, idx++);
    this.role = up.get(describedType, idx++);
    this.senderSettleMode = up.onUndef(describedType, idx++, constants.senderSettleMode.mixed);
    this.receiverSettleMode = up.onUndef(describedType, idx++, constants.receiverSettleMode.autoSettle);
    this.source = up.orNull(describedType, idx++);
    this.target = up.orNull(describedType, idx++);
    this.unsettled = up.onUndef(describedType, idx++, {});
    this.incompleteUnsettled = up.orFalse(describedType, idx++);
    this.initialDeliveryCount = up.get(describedType, idx++);
    this.maxMessageSize = up.onUndef(describedType, idx++, 0);
    this.offeredCapabilities = up.orNull(describedType, idx++);
    this.desiredCapabilities = up.orNull(describedType, idx++);
    this.properties = up.onUndef(describedType, idx++, {});
};

module.exports = AttachFrame;
