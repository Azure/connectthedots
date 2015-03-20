var EventEmitter    = require('events').EventEmitter,
    debug           = require('debug')('amqp10-client'),
    util            = require('util'),

    Connection      = require('./lib/connection'),
    M               = require('./lib/types/message'),
    Sasl            = require('./lib/sasl'),
    Session         = require('./lib/session').Session,
    Link            = require('./lib/session').Link,

    constants       = require('./lib/constants'),
    exceptions      = require('./lib/exceptions'),
    DescribedType   = require('./lib/types/described_type'),
    Fields          = require('./lib/types/amqp_composites').Fields,
    ForcedType      = require('./lib/types/forced_type'),
    Symbol          = require('./lib/types/symbol'),
    ST              = require('./lib/types/source_target'),
    DeliveryStates  = require('./lib/types/delivery_state'),
    Source          = ST.Source,
    Target          = ST.Target,

    Translator      = require('./lib/adapters/translate_encoder'),

    u               = require('./lib/utilities'),
    putils          = require('./lib/policies/policy_utilities');

/**
 * AMQPClient is the top-level class for interacting with node-amqp-1-0.  Instantiate this class, connect, and then send/receive
 * as needed and behind the scenes it will do the appropriate work to setup and teardown connections, sessions, and links and manage flow.
 * The code does its best to avoid exposing AMQP-specific types and attempts to convert them where possible, but on the off-chance you
 * need to speak AMQP-specific (e.g. to set a filter to a described-type), you can use node-amqp-encoder and the
 * AMQPClient.adapters.Translator adapter to convert it to our internal types.  See simple_eventhub_test.js for an example.
 *
 * Configuring AMQPClient is done through a Policy class.  By default, PolicyBase will be used - it assumes AMQP defaults wherever
 * possible, and for values with no spec-defined defaults it tries to assume something reasonable (e.g. timeout, max message size).
 *
 * To define a new policy, you can merge your values into an existing one by calling AMQPClient.policies.merge(yourPolicy, existingPolicy).
 * This does a deep-merge, allowing you to only replace values you need.  For instance, if you wanted the default sender settle policy to be auto-settle instead of mixed,
 * you could just use
 *
 <pre>
 var AMQPClient = require('node-amqp-1-0');
 var client = new AMQPClient(AMQPClient.policies.merge({
                  senderLinkPolicy: {
                    options: { senderSettleMode: AMQPClient.constants.senderSettleMode.settled } } });
 </pre>
 *
 * Obviously, setting some of these options requires some in-depth knowledge of AMQP, so I've tried to define specific policies where I can.
 * For instance, for Azure EventHub connections, you can use the pre-build EventHubPolicy.
 *
 * Also, within the policy, see the encoder and decoder defined in the send/receive policies.  These define what to do with the message
 * sent/received, and by default do a simple pass-through, leaving the encoding to/decoding from AMQP-specific types up to the library which
 * does a best-effort job.  See EventHubPolicy for a more complicated example, turning objects into UTF8-encoded buffers of JSON-strings.
 *
 * If, on construction, you provide a uri and a callback, I will immediately attempt to connect, allowing you to go directly from
 * instantiation to sending messages.
 *
 * @param {PolicyBase} [policy]     Policy to use for connection, sessions, links, etc.  Defaults to PolicyBase.
 * @param {string} [uri]            If provided, must provide cb.  Will attempt connection, set default queue.
 * @param {function} [cb]           If provided, must provide uri.  Will attempt connection and call cb when established/failed.
 * @constructor
 */
function AMQPClient(policy, uri, cb) {
    if (typeof policy === 'string') {
        cb = uri;
        uri = policy;
        policy = undefined;
    }
    // Make a protective copy
    this._originalPolicy = u.deepMerge(policy || PolicyBase);
    this.policy = u.deepMerge(this._originalPolicy);
    this._connection = null;
    this._reconnect = null;
    this._session = null;
    this._sendMsgId = 1;
    this._attaching = {};
    this._reattach = {};
    this._attached = {};
    this._onReceipt = {};
    this._pendingSends = {};
    this._unsettledSends = {};

    if (uri) {
        this.connect(uri, cb);
    }
}
util.inherits(AMQPClient, EventEmitter);

// Events - mostly for internal use.
AMQPClient.ErrorReceived = "ErrorReceived"; // Called with error
AMQPClient.ConnectionOpened = "Connection.Opened";
AMQPClient.SessionMapped = "Session.Mapped";
AMQPClient.LinkAttached = "Link.Attached"; // Called with link
AMQPClient.LinkDetached = "Link.Detached"; // Called with link
AMQPClient.SessionUnmapped = "Session.Unmapped";
AMQPClient.ConnectionClosed = "Connection.Closed";

/**
 * Exposes various AMQP-related constants, for use in policy overrides.
 */
AMQPClient.constants = constants;

/**
 * Map of various adapters from other AMQP-reliant libraries to the interface herein.
 *
 * Of primary interest in Translator, which allows you to translate from node-amqp-encoder'd values into the
 * internal types used in this library.  (e.g. [ 'symbol', 'symval' ] => Symbol('symval') ).
 */
AMQPClient.adapters = {
    'Translator': Translator
};

var PolicyBase      = require('./lib/policies/policy_base'),
    EHPolicy        = require('./lib/policies/event_hub_policy'),
    SBQueuePolicy   = require('./lib/policies/service_bus_queue_policy'),
    SBTopicPolicy   = require('./lib/policies/service_bus_topic_policy');

/**
 * Map of various pre-defined policies (including PolicyBase), as well as a merge function allowing you
 * to create your own.
 */
AMQPClient.policies = {
    'PolicyBase': PolicyBase,
    'EventHubPolicy': EHPolicy,
    'ServiceBusQueuePolicy': SBQueuePolicy,
    'ServiceBusTopicPolicy': SBTopicPolicy,
    merge: function(newPolicy, base) { return u.deepMerge(newPolicy, base || PolicyBase); },
    utils: putils
};

/**
 * Connects to a given AMQP server endpoint, and then calls the associated callback.  Sets the default queue, so e.g.
 * amqp://my-activemq-host/my-queue-name would set the default queue to my-queue-name for future send/receive calls.
 *
 * @param {string} url      URI to connect to - right now only supports <code>amqp|amqps</code> as protocol.
 * @param {function} cb     Callback to call on success - called with (error, self).
 */
AMQPClient.prototype.connect = function(url, cb) {
    if (this._connection) {
        this._connection.close();
        this._clearConnectionState();
    }

    debug('Connecting to ' + url);
    this._reconnect = this.connect.bind(this, url, function(){});
    var self = this;
    var address = u.parseAddress(url);
    this._defaultQueue = address.path.substr(1);
    this.policy.connectPolicy.options.hostname = address.host;
    var sasl = address.user ? new Sasl() : null;
    this._connection = this._newConnection();
    this._connection.on(Connection.Connected, function (c) {
        debug('Connected');
        self.emit(AMQPClient.ConnectionOpened);
        self._session = self._newSession(c);
        self._session.on(Session.Mapped, function (s) {
            debug('Mapped');
            cb(null, self);
            self.emit(AMQPClient.SessionMapped);
        });
        self._session.on(Session.Unmapped, function (s) {
            debug('Unmapped');
            self.emit(AMQPClient.SessionUnmapped);
        });
        self._session.on(Session.ErrorReceived, function (e) {
            debug('Session error: ', e);
            self.emit(AMQPClient.ErrorReceived, e);
        });
        self._session.on(Session.LinkAttached, function (l) {
            self.emit(AMQPClient.LinkAttached, l);
        });
        self._session.on(Session.LinkDetached, function (l) {
            debug('Link ' + l.name + ' detached');
            self.emit(AMQPClient.LinkDetached, l);
        });
        self._session.on(Session.DispositionReceived, function (details) {
            if (details.settled) {
                var err = null;
                if (details.state instanceof DeliveryStates.Rejected) {
                    err = details.state.error;
                }
                if (details.last) {
                    for (var msgid = details.first; msgid < details.last; ++msgid) {
                        if (self._unsettledSends[msgid]) {
                            self._unsettledSends[msgid](err, details.state);
                            self._unsettledSends[msgid] = undefined;
                        }
                    }
                } else {
                    if (self._unsettledSends[details.first]) {
                        self._unsettledSends[details.first](err, details.state);
                        self._unsettledSends[details.first] = undefined;
                    }
                }
            }
        });
        self._session.begin(self.policy.sessionPolicy);
    });
    this._connection.on(Connection.Disconnected, function() {
        debug('Disconnected');
        self.emit(AMQPClient.ConnectionClosed);
        if (self._shouldReconnect()) {
            self._attemptReconnection();
        } else {
            self._clearConnectionState(false);
        }
    });
    this._connection.on(Connection.ErrorReceived, function (e) {
        debug('Connection error: ', e);
        cb(e, self);
        self.emit(AMQPClient.ErrorReceived, e);
    });
    this._connection.open(address, sasl);
};

/**
 * Sends the given message, with the given annotations, to the given target.
 *
 * @param {*} msg               Message to send.  Will be encoded using sender link policy's encoder.
 * @param {string} [target]     Target to send to.  If not set, will use default queue from uri used to connect.
 * @param {*} [annotations]     Annotations for the message, if any.  See AMQP spec for details, and server for specific
 *                               annotations that might be relevant (e.g. x-opt-partition-key on EventHub).  If node-amqp-encoder'd
 *                               map is given, it will be translated to appropriate internal types.  Simple maps will be converted
 *                               to AMQP Fields type as defined in the spec.
 * @param {function} cb         Callback, by default called when settled disposition is received from target, with (error, delivery-state).
 *                              However, setting the sender callback policy to OnSent can change when this is called to as soon as the packets go out.
 */
AMQPClient.prototype.send = function(msg, target, annotations, cb) {

    var self = this;
    if (cb === undefined) {
        if (annotations === undefined) {
            cb = target;
            target = undefined;
        } else {
            if (typeof target === 'string') {
                cb = annotations;
                annotations = undefined;
            } else {
                cb = annotations;
                annotations = target;
                target = undefined;
            }
        }
    }

    if (!target) {
        target = this._defaultQueue;
    }

    // If given full address, pull out target first to avoid leaking credentials in error messages containing linkName.
    var rootUri;
    if (target && target.toLowerCase().lastIndexOf('amqp', 0) === 0) {
        var address = u.parseAddress(target);
        rootUri = address.rootUri;
        target = address.path.substring(1);
    }

    var linkName = target + "_TX";
    // Set some initial state for the link.
    if (this._pendingSends[linkName] === undefined) this._pendingSends[linkName] = [];
    if (this._attached[linkName] === undefined) this._attached[linkName] = null;
    if (this._attaching[linkName] === undefined) this._attaching[linkName] = false;

    var message = new M.Message();
    if (annotations) {
        // Convert encoded values
        if (annotations instanceof Array && annotations[0] === 'map') {
            annotations = AMQPClient.adapters.Translator(annotations);
        }
        message.annotations = new M.Annotations(annotations);
    }
    var enc = this.policy.senderLinkPolicy.encoder;
    message.body.push(enc ? enc(msg) : msg);
    var curId = self._sendMsgId++;

    var sender = function(err, _link) {
        if (_link.name === linkName) {
            if (err) {
                cb(err);
            } else {
                debug('Sending ', msg);
                var msgId = _link.sendMessage(message, {deliveryTag: new Buffer(curId.toString())});
                var cbPolicy = self.policy.senderLinkPolicy.callbackPolicy;
                if (cbPolicy === putils.SenderCallbackPolicies.OnSettle) {
                    self._unsettledSends[msgId] = cb;
                } else if (cbPolicy === putils.SenderCallbackPolicies.OnSent) {
                    cb(null);
                } else {
                    throw exceptions.ArgumentError('Invalid sender callback policy: ' + cbPolicy);
                }
            }
        }
    };

    if (this._attaching[linkName]) {
        // We're connecting, but our link isn't yet attached.  Add ourselves to the list for calling when attached.
        this._pendingSends[linkName].push(sender);
        return;
    }

    var attach = function() {
        self._attaching[linkName] = true;
        var onAttached = function (l) {
            if (l.name === linkName) {
                debug('Sender link ' + linkName + ' attached');
                self.removeListener(AMQPClient.LinkAttached, onAttached);
                self._attaching[linkName] = false;
                self._attached[linkName] = l;
                while (self._pendingSends[linkName] && self._pendingSends[linkName].length > 0 && l.canSend()) {
                    var curSend = self._pendingSends[linkName].shift();
                    curSend(null, l);
                }
                l.on(Link.ErrorReceived, function(err) {
                    if (self._pendingSends[linkName] && self._pendingSends[linkName].length > 0) {
                        for (var idx=0; idx < self._pendingSends.length; ++idx) {
                            self._pendingSends[idx](err, l);
                        }
                    }
                    self.emit(AMQPClient.ErrorReceived, err);
                });
                l.on(Link.CreditChange, function(_l) {
                    debug('Credit received');
                    while (self._pendingSends[linkName] && self._pendingSends[linkName].length > 0 && _l.canSend()) {
                        var curSend = self._pendingSends[linkName].shift();
                        curSend(null, _l);
                    }
                });
                l.on(Link.Detached, function(details) {
                    debug('Link detached: ' + (details ? details.error : 'No details'));
                    self._attached[linkName] = undefined;
                    if (self._pendingSends[linkName].length > 0) {
                        attach();
                    }
                });
            }
        };
        self.on(AMQPClient.LinkAttached, onAttached);
        if (self._session) {
            var linkPolicy = u.deepMerge({
                options: {
                    name: linkName,
                    source: {address: 'localhost'},
                    target: {address: target}
                }
            }, self.policy.senderLinkPolicy);
            self._session.attachLink(linkPolicy);
        } else {
            var onMapped = function() {
                self.removeListener(AMQPClient.SessionMapped, onMapped);
                var linkPolicy = u.deepMerge({
                    options: {
                        name: linkName,
                        source: {address: 'localhost'},
                        target: {address: target}
                    }
                }, self.policy.senderLinkPolicy);
                self._session.attachLink(linkPolicy);
            };
            self.on(AMQPClient.SessionMapped, onMapped);
        }
    };

    this._reattach[linkName] = attach;

    // If we're given a full address, ensure we're connected first.
    if (rootUri) {
        if (!this._attached[linkName]) {
            if (!this._connection) {
                this._attaching[linkName] = true;
                this._pendingSends[linkName].push(sender);

                // If we're not connected yet, connect, then callback into ourselves.
                this.connect(rootUri, function (conn_err) {
                    if (conn_err) {
                        cb(conn_err);
                    } else {
                        attach();
                    }
                });
                return;
            } else {
                // We must've dropped our link, but connection is still active.  Try and re-establish.
                self._pendingSends[linkName].push(sender);
                attach();
                return;
            }
        }
    } else {
        if (!this._attached[linkName]) {
            self._pendingSends[linkName].push(sender);
            attach();
            return;
        }
    }

    var link = this._attached[linkName];
    if (link.canSend()) {
        sender(null, link);
    } else {
        this._pendingSends[linkName].push(sender);
    }
};

/**
 * Set up callback to be called whenever message is received from the given source (subject to the given filter).
 * Callback is called with (error, payload, annotations), and the payload is decoded using the receiver link policy's
 * decoder method.
 *
 * @param {string} [source]     Source of the link to connect to.  If not provided will use default queue from connection uri.
 * @param {*} [filter]          Filter used in connecting to the source.  See AMQP spec for details, and your server's documentation
 *                               for possible values.  node-amqp-encoder'd maps will be translated, and simple maps will be converted
 *                               to AMQP Fields type as defined in the spec.
 * @param {function} cb         Callback to invoke on every receipt.  Called with (error, payload, annotations).
 */
AMQPClient.prototype.receive = function(source, filter, cb) {
    var self = this;
    if (cb === undefined) {
        if (typeof source === 'function') {
            cb = source;
            source = this._defaultQueue;
            filter = undefined;
        } else if (typeof source !== 'string') {
            cb = filter;
            filter = source;
            source = this._defaultQueue;
        } else {
            cb = filter;
            filter = undefined;
        }
    }
    if (filter && filter instanceof Array && filter[0] === 'map') {
        // Convert encoded values
        filter = AMQPClient.adapters.Translator(filter);
    }

    // If given full address, pull out source first to avoid leaking credentials in error messages containing linkName.
    var rootUri;
    if (source && source.toLowerCase().lastIndexOf('amqp', 0) === 0) {
        var address = u.parseAddress(source);
        rootUri = address.rootUri;
        source = address.path.substring(1);
    }

    var linkName = source + "_RX";
    // Set some initial state for the link.
    if (this._onReceipt[linkName] === undefined) this._onReceipt[linkName] = [];
    if (this._attached[linkName] === undefined) this._attached[linkName] = null;
    if (this._attaching[linkName] === undefined) this._attaching[linkName] = false;

    this._onReceipt[linkName].push(cb);
    if (this._attaching[linkName] || this._attached[linkName]) return;

    var attach = function() {
        var onAttached = function (l) {
            if (l.name === linkName) {
                debug('Receiver link ' + linkName + ' attached');
                self.removeListener(AMQPClient.LinkAttached, onAttached);
                self._attaching[linkName] = false;
                self._attached[linkName] = l;
                l.on(Link.ErrorReceived, function (err) {
                    var cbs = self._onReceipt[linkName];
                    if (cbs && cbs.length > 0) {
                        for (var idx = 0; idx < cbs.length; ++idx) {
                            cbs[idx](err);
                        }
                    }
                });
                l.on(Link.MessageReceived, function (m) {
                    var payload = m.body[0];
                    var decoded = l.policy.decoder ? l.policy.decoder(payload) : payload;
                    debug('Received ' + decoded + ' from ' + source);
                    var cbs = self._onReceipt[linkName];
                    if (cbs && cbs.length > 0) {
                        for (var idx = 0; idx < cbs.length; ++idx) {
                            cbs[idx](null, decoded, m.annotations);
                        }
                    }
                });
                l.on(Link.Detached, function(details) {
                    debug('Link detached: ' + (details ? details.error : 'No details'));
                    self._attached[linkName] = undefined;
                    attach();
                });
            }
        };
        self.on(AMQPClient.LinkAttached, onAttached);
        if (self._session) {
            var linkPolicy = u.deepMerge({
                options: {
                    name: linkName,
                    source: {address: source, filter: filter},
                    target: {address: 'localhost'}
                }
            }, self.policy.receiverLinkPolicy);
            self._session.attachLink(linkPolicy);
        } else {
            var onMapped = function() {
                self.removeListener(AMQPClient.SessionMapped, onMapped);
                var linkPolicy = u.deepMerge({
                    options: {
                        name: linkName,
                        source: {address: source, filter: filter},
                        target: {address: 'localhost'}
                    }
                }, self.policy.receiverLinkPolicy);
                self._session.attachLink(linkPolicy);
            };
            self.on(AMQPClient.SessionMapped, onMapped);
        }
    };

    this._reattach[linkName] = attach;

    // If we're given a full address, ensure we're connected first.
    if (rootUri) {
        if (!this._attached[linkName]) {
            if (!this._connection) {
                // If we're not connected yet, connect, then callback into ourselves.
                this.connect(rootUri, function (conn_err) {
                    if (conn_err) {
                        cb(conn_err);
                    } else {
                        attach();
                    }
                });
            } else {
                // We must've dropped our link, but connection is still active.  Try and re-establish.
                attach();
            }
        }
    } else {
        if (!this._attached[linkName]) {
            attach();
        }
    }
};

/**
 * Disconnect tears down any existing connection with appropriate Close performatives and TCP socket teardowns.
 *
 * @param {function} cb     Called when connection is completely disconnected.
 */
AMQPClient.prototype.disconnect = function(cb) {
    debug('Disconnecting');
    if (this._connection) {
        var self = this;
        this._preventReconnect();
        this._connection.on(Connection.Disconnected, function() {
            self._connection = null;
            cb();
        });
        this._connection.close();
        this._clearConnectionState();
    } else {
        cb(); // Already disconnected, just call the callback.
    }
};

AMQPClient.prototype._clearConnectionState = function(saveReconnectDetails) {
    this._attached = {};
    this._attaching = {};
    this._unsettledSends = {};
    this._connection = null;
    this._session = null;
    // Copy from original to avoid any settings changes "sticking" across connections.
    this.policy = u.deepMerge(this._originalPolicy);

    if (!saveReconnectDetails) {
        this._pendingSends = {};
        this._onReceipt = {};
        this._reattach = {};
        this._reconnect = null;
    }
};

// Helper methods for mocking in tests.
AMQPClient.prototype._newConnection = function() {
    return new Connection(this.policy.connectPolicy);
};

AMQPClient.prototype._newSession = function(conn) {
    return new Session(conn);
};

AMQPClient.prototype._preventReconnect = function() {
    this._reconnect = null;
    this._reattach = {};
    this._pendingSends = {};
    this._onReceipt = {};
};

AMQPClient.prototype._shouldReconnect = function() {
    if (!this._connection || !this._reconnect) return false;
    if (Object.keys(this._onReceipt).length > 0) return true;

    var pendingSends = 0;
    for (var k in this._pendingSends) pendingSends += this._pendingSends[k].length;
    return pendingSends > 0;
};

AMQPClient.prototype._attemptReconnection = function() {
    this._clearConnectionState(true);
    var self = this;
    var onReconnect = function() {
        debug('Reconnected and remapped, attempting to re-attach links.');
        self.removeListener(AMQPClient.SessionMapped, onReconnect);
        for (var ln in self._reattach) {
            debug('Reattaching ' + ln);
            self._reattach[ln]();
        }
    };
    self.on(AMQPClient.SessionMapped, onReconnect);
    self._reconnect();
};

module.exports = AMQPClient;
