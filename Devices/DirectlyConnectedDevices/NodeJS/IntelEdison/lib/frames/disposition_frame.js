var debug       = require('debug')('amqp10-DispositionFrame'),
    util        = require('util'),
    Int64       = require('node-int64'),

    constants   = require('../constants'),
    u           = require('../utilities'),
    up          = u.payload,

    DescribedType   = require('../types/described_type'),
    ForcedType  = require('../types/forced_type'),
    Symbol      = require('../types/symbol'),

    FrameBase   = require('./frame');

/**
 * <h2>disposition performative</h2>
 * <i>inform remote peer of delivery state changes</i>
 * <p>
 *           The disposition frame is used to inform the remote peer of local changes in the state of
 *           deliveries. The disposition frame may reference deliveries from many different links
 *           associated with a session, although all links MUST have the directionality indicated by
 *           the specified .
 *         </p>
 * <p>
 *           Note that it is possible for a disposition sent from sender to receiver to refer to a
 *           delivery which has not yet completed (i.e. a delivery which is spread over multiple
 *           frames and not all frames have yet been sent). The use of such interleaving is
 *           discouraged in favor of carrying the modified state on the next
 *           performative for the delivery.
 *         </p>
 * <p>transfer</p>
 * <p>
 *           The disposition performative may refer to deliveries on links that are no longer attached.
 *           As long as the links have not been closed or detached with an error then the deliveries
 *           are still "live" and the updated state MUST be applied.
 *         </p>
 * <h3>Descriptor</h3>
 * <dl>
 * <dt>Name</dt>
 * <dd>amqp:disposition:list</dd>
 * <dt>Code</dt>
 * <dd>0x00000000:0x00000015</dd>
 * </dl>
 *
 * <table border="1">
 * <tr><th>Name</th><th>Type</th><th>Mandatory?</th><th>Multiple?</th></tr><tr><td>role</td><td>role</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>directionality of disposition</i>
 * <p>
 *             The role identifies whether the disposition frame contains information
 *             about  link endpoints or  link endpoints.
 *           </p></td></tr>
 * <tr><td>first</td><td>delivery-number</td><td>true</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>lower bound of deliveries</i>
 * <p>
 *             Identifies the lower bound of delivery-ids for the deliveries in this set.
 *           </p></td></tr>
 * <tr><td>last</td><td>delivery-number</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>upper bound of deliveries</i>
 * <p>
 *             Identifies the upper bound of delivery-ids for the deliveries in this set. If not set,
 *             this is taken to be the same as .
 *           </p></td></tr>
 * <tr><td>settled</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates deliveries are settled</i>
 * <p>
 *             If true, indicates that the referenced deliveries are considered settled by the issuing
 *             endpoint.
 *           </p></td></tr>
 * <tr><td>state</td><td>*</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>indicates state of deliveries</i>
 * <p>
 *             Communicates the state of all the deliveries referenced by this disposition.
 *           </p></td></tr>
 * <tr><td>batchable</td><td>boolean</td><td>false</td><td>false</td></tr>
 * <tr><td>&nbsp;</td><td colspan="3"><i>batchable hint</i>
 * <p>
 *             If true, then the issuer is hinting that there is no need for the peer to urgently
 *             communicate the impact of the updated delivery states. This hint may be used to
 *             artificially increase the amount of batching an implementation uses when communicating
 *             delivery states, and thereby save bandwidth.
 *           </p></td></tr>
 * </table>
 *
 * @constructor
 */
function DispositionFrame(options) {
    DispositionFrame.super_.call(this);
    this.channel = 0;
    if (options instanceof DescribedType) {
        this.readPerformative(options);
    } else {
        exceptions.assertArguments(options, ['role', 'first']);
        this.role = options.role;
        this.first = options.first;
        this.last = u.onUndef(options.last, 0);
        this.settled = u.orFalse(options.settled);
        this.state = u.orNull(options.state);
        this.batchable = u.orFalse(options.batchable);
    }
}

util.inherits(DispositionFrame, FrameBase.AMQPFrame);

DispositionFrame.Descriptor = {
    name: new Symbol('amqp:disposition:list'),
    code: new Int64(0x00000000, 0x00000015)
};

DispositionFrame.prototype._getPerformative = function() {
    var self = this;
    return new DescribedType(BeginFrame.Descriptor.code, {
        role: self.role,
        first: new ForcedType('uint', self.first),
        last: new ForcedType('uint', self.last),
        settled: self.settled,
        state: self.state,
        batchable: self.batchable,
        encodeOrdering: [ 'role', 'first', 'last', 'settled', 'state', 'batchable']
    });
};

DispositionFrame.prototype.readPerformative = function(describedType) {
    up.assert(describedType, 0, 'role');
    up.assert(describedType, 1, 'first');

    var idx = 0;
    this.role = up.get(describedType, idx++);
    this.first = up.get(describedType, idx++);
    this.last = up.get(describedType, idx++);
    this.settled = up.orFalse(describedType, idx++);
    this.state = up.get(describedType, idx++);
    this.batchable = up.orFalse(describedType, idx++);
};

module.exports = DispositionFrame;
