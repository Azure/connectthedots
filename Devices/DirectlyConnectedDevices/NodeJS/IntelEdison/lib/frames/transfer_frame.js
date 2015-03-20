var debug       = require('debug')('amqp10-TransferFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),
    Builder     = require('node-amqp-encoder').Builder,

    constants   = require('../constants'),
    exceptions  = require('../exceptions'),
    u           = require('../utilities'),
    up          = u.payload,

    DescribedType   = require('../types/described_type'),
    ForcedType  = require('../types/forced_type'),
    Symbol      = require('../types/symbol'),

    FrameBase   = require('./frame');

/**
 * <h2>transfer performative</h2>
 * <i>transfer a Message</i>
 * <p>
 *           The transfer frame is used to send Messages across a Link. Messages may be carried by a
 *           single transfer up to the maximum negotiated frame size for the Connection. Larger
 *           Messages may be split across several transfer frames.
 *         </p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:transfer:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000014</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>handle</td><td>handle</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>Specifies the Link on which the Message is transferred.</p></td></tr>
 * <tr><td>delivery-id</td><td>delivery-number</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>alias for delivery-tag</i>
 * <p>
 *             The delivery-id MUST be supplied on the first transfer of a multi-transfer delivery. On
 *             continuation transfers the delivery-id MAY be omitted. It is an error if the delivery-id
 *             on a continuation transfer differs from the delivery-id on the first transfer of a
 *             delivery.
 *           </p></td></tr>
 * <tr><td>delivery-tag</td><td>delivery-tag</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             Uniquely identifies the delivery attempt for a given Message on this Link. This field
 *             MUST be specified for the first transfer of a multi transfer message and may only be
 *             omitted for continuation transfers.
 *           </p></td></tr>
 * <tr><td>message-format</td><td>message-format</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates the message format</i>
 * <p>
 *             This field MUST be specified for the first transfer of a multi transfer message and may
 *             only be omitted for continuation transfers.
 *           </p></td></tr>
 * <tr><td>settled</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *              If not set on the first (or only) transfer for a delivery, then the settled flag MUST
 *              be interpreted as being false. For subsequent transfers if the settled flag is left
 *              unset then it MUST be interpreted as true if and only if the value of the settled flag
 *              on any of the preceding transfers was true; if no preceding transfer was sent with
 *              settled being true then the value when unset MUST be taken as false.
 *           </p>
 * <p>
 *              If the negotiated value for snd-settle-mode at attachment is , then this field MUST be true on at least
 *              one transfer frame for a delivery (i.e. the delivery must be settled at the Sender at
 *              the point the delivery has been completely transferred).
 *           </p>
 * <p>sender-settle-mode</p>
 * <p>
 *              If the negotiated value for snd-settle-mode at attachment is , then this field MUST be false (or
 *              unset) on every transfer frame for a delivery (unless the delivery is aborted).
 *           </p>
 * <p>sender-settle-mode</p></td></tr>
 * <tr><td>more</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates that the Message has more content</i>
 * <p>
 *             Note that if both the more and aborted fields are set to true, the aborted flag takes
 *             precedence. That is a receiver should ignore the value of the more field if the
 *             transfer is marked as aborted. A sender SHOULD NOT set the more flag to true if it
 *             also sets the aborted flag to true.
 *           </p></td></tr>
 * <tr><td>rcv-settle-mode</td><td>receiver-settle-mode</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3">undefined
 * <p>
 *             If , this indicates that the
 *             Receiver MUST settle the delivery once it has arrived without waiting for the Sender to
 *             settle first.
 *           </p>
 * <p>receiver-settle-mode</p>
 * <p>
 *             If , this indicates that the
 *             Receiver MUST NOT settle until sending its disposition to the Sender and receiving a
 *             settled disposition from the sender.
 *           </p>
 * <p>receiver-settle-mode</p>
 * <p>
 *             If not set, this value is defaulted to the value negotiated on link attach.
 *           </p>
 * <p>
 *             If the negotiated link value is ,
 *             then it is illegal to set this field to .
 *           </p>
 * <p>receiver-settle-mode</p>
 * <p>
 *             If the message is being sent settled by the Sender, the value of this field is ignored.
 *           </p>
 * <p>
 *             The (implicit or explicit) value of this field does not form part of the transfer state,
 *             and is not retained if a link is suspended and subsequently resumed.
 *           </p></td></tr>
 * <tr><td>state</td><td>*</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>the state of the delivery at the sender</i>
 * <p>
 *             When set this informs the receiver of the state of the delivery at the sender. This is
 *             particularly useful when transfers of unsettled deliveries are resumed after a resuming
 *             a link. Setting the state on the transfer can be thought of as being equivalent to
 *             sending a disposition immediately before the  performative, i.e.
 *             it is the state of the delivery (not the transfer) that existed at the point the frame
 *             was sent.
 *           </p>
 * <p>transfer</p>
 * <p>
 *             Note that if the  performative (or an earlier  performative referring to the delivery) indicates that the delivery
 *             has attained a terminal state, then no future  or  sent by the sender can alter that terminal state.
 *           </p>
 * <p>transfer</p></td></tr>
 * <tr><td>resume</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates a resumed delivery</i>
 * <p>
 *             If true, the resume flag indicates that the transfer is being used to reassociate an
 *             unsettled delivery from a dissociated link endpoint. See
 *              for more details.
 *           </p>
 * <p>resuming-deliveries</p>
 * <p>
 *             The receiver MUST ignore resumed deliveries that are not in its local unsettled map. The
 *             sender MUST NOT send resumed transfers for deliveries not in its local unsettled map.
 *           </p>
 * <p>
 *             If a resumed delivery spans more than one transfer performative, then the resume flag
 *             MUST be set to true on the first transfer of the resumed delivery.  For subsequent
 *             transfers for the same delivery the resume flag may be set to true, or may be omitted.
 *           </p>
 * <p>
 *             In the case where the exchange of unsettled maps makes clear that all message data has
 *             been successfully transferred to the receiver, and that only the final state (and
 *             potentially settlement) at the sender needs to be conveyed, then a resumed delivery may
 *             carry no payload and instead act solely as a vehicle for carrying the terminal state of
 *             the delivery at the sender.
 *            </p></td></tr>
 * <tr><td>aborted</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates that the Message is aborted</i>
 * <p>
 *             Aborted Messages should be discarded by the recipient (any payload within the frame
 *             carrying the performative MUST be ignored). An aborted Message is implicitly settled.
 *           </p></td></tr>
 * <tr><td>batchable</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>batchable hint</i>
 * <p>
 *             If true, then the issuer is hinting that there is no need for the peer to urgently
 *             communicate updated delivery state. This hint may be used to artificially increase the
 *             amount of batching an implementation uses when communicating delivery states, and
 *             thereby save bandwidth.
 *           </p>
 * <p>
 *             If the message being delivered is too large to fit within a single frame, then the
 *             setting of batchable to true on any of the  performatives for the
 *             delivery is equivalent to setting batchable to true for all the
 *             performatives for the delivery.
 *           </p>
 * <p>transfer</p>
 * <p>
 *             The batchable value does not form part of the transfer state, and is not retained if
 *             a link is suspended and subsequently resumed.
 *           </p></td></tr>
 * </table>
 *
 * @constructor
 */
function TransferFrame(options) {
    TransferFrame.super_.call(this);
    this.channel = 0;
    if (options instanceof DescribedType) {
        this.readPerformative(options);
    } else {
        exceptions.assertArguments(options, ['handle']);

        this.handle = options.handle;
        this.deliveryId = u.orNull(options.deliveryId);
        this.deliveryTag = u.orNull(options.deliveryTag);
        this.messageFormat = u.orNull(options.messageFormat);
        this.settled = u.orNull(options.settled);
        this.more = u.orFalse(options.more);
        this.receiverSettleMode = u.orNull(options.receiverSettleMode);
        this.state = u.orNull(options.state);
        this.resume = u.orFalse(options.resume);
        this.aborted = u.orFalse(options.aborted);
        this.batchable = u.orFalse(options.batchable);

        this.message = options.message;
    }
}

util.inherits(TransferFrame, FrameBase.AMQPFrame);

TransferFrame.Descriptor = {
    name: new Symbol('amqp:transfer:list'),
    code: new Int64(0x00000000, 0x00000014)
};

TransferFrame.prototype._getPerformative = function() {
    var self = this;
    //return new Builder().
    //    described().$ulong(0x14).
    //    list().
    //      $uint(self.handle).
    //      $uint(self.deliverId).
    //      binary(self.deliveryTag).
    //      $uint(self.messageFormat).
    //      boolean(self.settled).
    //      boolean(self.more).
    //      $ubyte(self.receiverSettleMode).
    //      append(u.encode(self.state)).
    //      boolean(self.resume).
    //      boolean(self.aborted).
    //      boolean(self.batchable).
    //    end().encode();
    //
    return new DescribedType(TransferFrame.Descriptor.code, {
        handle: new ForcedType('uint', self.handle),
        deliveryId: new ForcedType('uint', self.deliveryId),
        deliveryTag: self.deliveryTag,
        messageFormat: new ForcedType('uint', self.messageFormat),
        settled: self.settled,
        more: self.more,
        receiverSettleMode: new ForcedType('ubyte', self.receiverSettleMode),
        state: self.state,
        resume: self.resume,
        aborted: self.aborted,
        batchable: self.batchable,
        encodeOrdering: [ 'handle', 'deliveryId', 'deliveryTag', 'messageFormat', 'settled', 'more',
            'receiverSettleMode', 'state', 'resume', 'aborted', 'batchable' ]
    });
};

TransferFrame.prototype.readPerformative = function(describedType) {
    up.assert(describedType, 0, 'handle');

    var idx = 0;
    this.handle = up.get(describedType, idx++);
    this.deliveryId = up.orNull(describedType, idx++);
    this.deliveryTag = up.orNull(describedType, idx++);
    this.messageFormat = up.orNull(describedType, idx++);
    this.settled = up.orNull(describedType, idx++);
    this.more = up.orFalse(describedType, idx++);
    this.receiverSettleMode = up.orNull(describedType, idx++);
    this.state = up.orNull(describedType, idx++);
    this.resume = up.orFalse(describedType, idx++);
    this.aborted = up.orFalse(describedType, idx++);
    this.batchable = up.orFalse(describedType, idx++);
};

TransferFrame.prototype._getAdditionalPayload = function() {
    return this.message;
};

module.exports = TransferFrame;
