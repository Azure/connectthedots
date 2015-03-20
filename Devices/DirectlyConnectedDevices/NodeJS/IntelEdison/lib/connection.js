var EventEmitter    = require('events').EventEmitter,
    fs          = require('fs'),
    net         = require('net'),
    tls         = require('tls'),
    util        = require('util'),

    debug       = require('debug')('amqp10-Connection'),
    cbuf        = require('cbarrick-circular-buffer'),
    StateMachine= require('stately.js'),

    constants   = require('./constants'),
    codec       = require('./codec'),
    exceptions  = require('./exceptions'),
    utils       = require('./utilities'),

    AMQPError   = require('./types/amqp_error'),
    DescribedType   = require('./types/described_type'),

    FrameReader = require('./frames/frame_reader'),
    CloseFrame  = require('./frames/close_frame'),
    HeartbeatFrame  = require('./frames/heartbeat_frame'),
    OpenFrame   = require('./frames/open_frame');

/**
 * Connection states, from AMQP 1.0 spec:
 *
 <dl>
 <dt>START</dt>
 <dd><p>In this state a Connection exists, but nothing has been sent or received. This is the
 state an implementation would be in immediately after performing a socket connect or
 socket accept.</p></dd>

 <dt>HDR-RCVD</dt>
 <dd><p>In this state the Connection header has been received from our peer, but we have not
 yet sent anything.</p></dd>

 <dt>HDR-SENT</dt>
 <dd><p>In this state the Connection header has been sent to our peer, but we have not yet
 received anything.</p></dd>

 <dt>OPEN-PIPE</dt>
 <dd><p>In this state we have sent both the Connection header and the open frame, but we have not yet received anything.
 </p></dd>

 <dt>OC-PIPE</dt>
 <dd><p>In this state we have sent the Connection header, the open
 frame, any pipelined Connection traffic, and the close frame,
 but we have not yet received anything.</p></dd>

 <dt>OPEN-RCVD</dt>
 <dd><p>In this state we have sent and received the Connection header, and received an
 open frame from our peer, but have not yet sent an
 open frame.</p></dd>

 <dt>OPEN-SENT</dt>
 <dd><p>In this state we have sent and received the Connection header, and sent an
 open frame to our peer, but have not yet received an
 open frame to our peer, but have not yet received an
 open frame.</p></dd>

 <dt>CLOSE-PIPE</dt>
 <dd><p>In this state we have send and received the Connection header, sent an
 open frame, any pipelined Connection traffic, and the
 close frame, but we have not yet received an
 open frame.</p></dd>

 <dt>OPENED</dt>
 <dd><p>In this state the Connection header and the open frame
 have both been sent and received.</p></dd>

 <dt>CLOSE-RCVD</dt>
 <dd><p>In this state we have received a close frame indicating
 that our partner has initiated a close. This means we will never have to read anything
 more from this Connection, however we can continue to write frames onto the Connection.
 If desired, an implementation could do a TCP half-close at this point to shutdown the
 read side of the Connection.</p></dd>

 <dt>CLOSE-SENT</dt>
 <dd><p>In this state we have sent a close frame to our partner.
 It is illegal to write anything more onto the Connection, however there may still be
 incoming frames. If desired, an implementation could do a TCP half-close at this point
 to shutdown the write side of the Connection.</p></dd>

 <dt>DISCARDING</dt>
 <dd><p>The DISCARDING state is a variant of the CLOSE_SENT state where the
 close is triggered by an error. In this case any incoming frames on
 the connection MUST be silently discarded until the peer's close frame
 is received.</p></dd>

 <dt>END</dt>
 <dd><p>In this state it is illegal for either endpoint to write anything more onto the
 Connection. The Connection may be safely closed and discarded.</p></dd>
 </dl>
 *
 * Connection negotiation state diagram from AMQP 1.0 spec:
 *
 <pre>
              R:HDR +=======+ S:HDR             R:HDR[!=S:HDR]
           +--------| START |-----+    +--------------------------------+
           |        +=======+     |    |                                |
          \\|/                    \\|/   |                                |
      +==========+             +==========+ S:OPEN                      |
 +----| HDR-RCVD |             | HDR-SENT |------+                      |
 |    +==========+             +==========+      |      R:HDR[!=S:HDR]  |
 |   S:HDR |                      | R:HDR        |    +-----------------+
 |         +--------+      +------+              |    |                 |
 |                 \\|/    \\|/                   \\|/   |                 |
 |                +==========+               +-----------+ S:CLOSE      |
 |                | HDR-EXCH |               | OPEN-PIPE |----+         |
 |                +==========+               +-----------+    |         |
 |           R:OPEN |      | S:OPEN              | R:HDR      |         |
 |         +--------+      +------+      +-------+            |         |
 |        \\|/                    \\|/    \\|/                  \\|/        |
 |   +===========+             +===========+ S:CLOSE       +---------+  |
 |   | OPEN-RCVD |             | OPEN-SENT |-----+         | OC-PIPE |--+
 |   +===========+             +===========+     |         +---------+  |
 |  S:OPEN |                      | R:OPEN      \\|/           | R:HDR   |
 |         |       +========+     |          +------------+   |         |
 |         +----- >| OPENED |< ---+          | CLOSE-PIPE |< -+         |
 |                 +========+                +------------+             |
 |           R:CLOSE |    | S:CLOSE              | R:OPEN               |
 |         +---------+    +-------+              |                      |
 |        \\|/                    \\|/             |                      |
 |   +============+          +=============+     |                      |
 |   | CLOSE-RCVD |          | CLOSE-SENT* |< ---+                      |
 |   +============+          +=============+                            |
 | S:CLOSE |                      | R:CLOSE                             |
 |         |         +=====+      |                                     |
 |         +------- >| END |< ----+                                     |
 |                   +=====+                                            |
 |                     /|\                                              |
 |    S:HDR[!=R:HDR]    |                R:HDR[!=S:HDR]                 |
 +----------------------+-----------------------------------------------+

 </pre>
 *
 * R:<b>CTRL</b> = Received <b>CTRL</b>
 *
 * S:<b>CTRL</b> = Sent <b>CTRL</b>
 *
 * Also could be DISCARDING if an error condition triggered the CLOSE
 *
 * @param connectPolicy ConnectPolicy from a PolicyBase implementation
 * @constructor
 */
function Connection(connectPolicy) {
    Connection.super_.call(this);
    var options = connectPolicy.options;
    exceptions.assertArguments(options, [ 'containerId', 'hostname' ]);
    this.policy = connectPolicy;
    this.client = null;
    this.connected = false;
    this.connectedTo = null;
    this._curbuf = new cbuf({ size: 1024, encoding: 'buffer' });
    this.connectionParams = {
        containerId: (typeof options.containerId === 'function') ? options.containerId() : options.containerId,
        hostname: options.hostname,
        maxFrameSize: options.maxFrameSize || constants.defaultMaxFrameSize,
        channelMax: options.channelMax || constants.defaultChannelMax,
        idleTimeout: options.idleTimeout || constants.defaultIdleTimeout,
        outgoingLocales: options.outgoingLocales,
        incomingLocales: options.incomingLocales
    };
    this._sslOptions = null;
    if (options.sslOptions) {
        this._sslOptions = {};
        if (options.sslOptions.keyFile) {
            this._sslOptions.key = fs.readFileSync(options.sslOptions.keyFile);
        }
        if (options.sslOptions.certFile) {
            this._sslOptions.cert = fs.readFileSync(options.sslOptions.certFile);
        }
        if (options.sslOptions.caFile) {
            this._sslOptions.ca = fs.readFileSync(options.sslOptions.caFile);
        }
        this._sslOptions.rejectUnauthorized = options.sslOptions.rejectUnauthorized;
    }

    this._lastOutgoing = null; // To track whether heartbeat frame required.
    this._receivedHeader = false;
    this._sessions = {};
    var self = this;
    var stateMachine = {
        'DISCONNECTED': {
            connect: function(address, sasl) {
                self._connect(address, sasl);
                return this.START;
            }
        },
        'START': {
            connected: function(sasl) {
                if (sasl) {
                    self.sasl = sasl;
                    self.sasl.negotiate(self, self.address, function(e){ self._saslComplete(e); });
                    return this.IN_SASL;
                } else {
                    self.sendHeader();
                    return this.HDR_SENT;
                }
            },
            headerReceived: function() { return this.HDR_RCVD; }
        },
        'IN_SASL': {
            success: function() {
                self.sendHeader();
                return this.HDR_SENT;
            }
        },
        'HDR_RCVD': {
            validVersion: function() {
                self.sendHeader();
                return this.HDR_EXCH;
            },
            invalidVersion: function() {
                self._terminate();
                return this.DISCONNECTING;
            }
        },
        'HDR_SENT': {
            validVersion: function() {
                return this.HDR_EXCH;
            },
            invalidVersion: function() {
                self._terminate();
                return this.DISCONNECTING;
            }
        },
        'HDR_EXCH': {
            sendOpen: function() {
                self._sendOpenFrame();
                return this.OPEN_SENT;
            },
            openReceived: function() {
                return this.OPEN_RCVD;
            }
        },
        'OPEN_RCVD': {
            sendOpen: function() {
                self._sendOpenFrame();
                return this.OPENED;
            }
        },
        'OPEN_SENT': {
            openReceived: function() {
                return this.OPENED;
            },
            sendClose: function() {
                self._terminate();
                return this.DISCONNECTED;
            }
        },
        'OPENED': {
            closeReceived: function() {
                return this.CLOSE_RCVD;
            },
            invalidOptions: function() {
                self._sendCloseFrame(new AMQPError(AMQPError.ConnectionForced, 'Invalid options'));
                return this.CLOSE_SENT;
            },
            sendClose: function() {
                self._sendCloseFrame();
                return this.CLOSE_SENT;
            }
        },
        'CLOSE_RCVD': {
            sendClose: function() {
                self._sendCloseFrame();
                self._terminate();
                return this.DISCONNECTED;
            }
        },
        'CLOSE_SENT': {
            closeReceived: function() {
                self._terminate();
                return this.DISCONNECTED;
            },
            disconnect: function() { return this.DISCONNECTED; }
        },
        'DISCONNECTING': {
            disconnected: function() {
                return this.DISCONNECTED;
            }
        }
    };

    var errorHandler = function(err) { self._processError(err); return this.DISCONNECTED; };
    var terminationHandler = function() { self._terminate(); return this.DISCONNECTED; };
    Object.keys(stateMachine).forEach(function (state) {
        if (state !== 'DISCONNECTED') {
            stateMachine[state].error = errorHandler;
            stateMachine[state].terminated = terminationHandler;
        }
    });

    this.connSM = new StateMachine(stateMachine).bind(function(event, oldState, newState) {
        debug('Transitioning from '+oldState+' to '+newState+' due to '+event);
    });
}

util.inherits(Connection, EventEmitter);

// Events
Connection.Connected = 'connected';
Connection.Disconnected = 'disconnected';
// On receipt of a frame not handled internally (e.g. not a BEGIN/CLOSE/SASL).  Provides received frame as an argument.
Connection.FrameReceived = 'rxFrame';
// Since 'error' events are "special" in Node (as in halt-the-process special), using a custom event for errors
// we receive from the other endpoint.  Provides received AMQPError as an argument.
Connection.ErrorReceived = 'rxError';

/**
 * Open a connection to the given (parsed) address (@see {@link AMQPClient}).
 *
 * @param address   Contains at least protocol, host and port, may contain user/pass, path.
 * @param sasl      If given, contains a "negotiate" method that, given address and a callback, will run through SASL negotiations.
 */
Connection.prototype.open = function(address, sasl) {
    exceptions.assertArguments(address, ['protocol', 'host', 'port']);
    this.connSM.connect(address, sasl);
};

Connection.prototype.close = function() {
    this.connSM.sendClose();
};

Connection.prototype.sendFrame = function(frame) {
    var frameBuf = frame.outgoing();
    debug('Sending frame: ' + frame.constructor.name + ': ' + JSON.stringify(frame) + ': ' + frameBuf.toString('hex'));
    this._lastOutgoing = Date.now();
    this.client.write(frameBuf);
};

Connection.prototype.associateSession = function(session) {
    var channel = this._nextChannel();
    this._sessions[channel] = session;
    return channel;
};

Connection.prototype.dissociateSession = function(channel) {
    this._sessions[channel] = undefined;
};

Connection.prototype._nextChannel = function() {
    for (var cid = 1; cid <= this.connectionParams.channelMax; ++cid) {
        if (this._sessions[cid] === undefined) return cid;
    }
    throw new exceptions.OverCapacityError('Out of available ' + this.connectionParams.channelMax + ' channels');
};

Connection.prototype._processError = function(err) {
    console.warn('Error from socket: '+err);
    // TODO: Cleanly close on error
    this._terminate();
};

Connection.prototype._receiveData = function(buffer) {
    debug('Rx: ' + buffer.toString('hex'));
    this._curbuf.write(buffer);
    this._receiveAny();
};

Connection.prototype.sendHeader = function(header) {
    header = header || constants.amqpVersion;
    debug('Sending Header ' + header.toString('hex'));
    this.client.write(header);
};

Connection.prototype._tryReceiveHeader = function(header) {
    header = header || constants.amqpVersion;
    if (this._curbuf.length >= header.length) {
        var serverVersion = this._curbuf.read(8);
        if (this.sasl) {
            this.sasl.headerReceived(serverVersion);
        } else {
            debug('Server AMQP Version: ' + serverVersion.toString('hex') + ' vs ' + header.toString('hex'));
            if (this.connSM.getMachineState() === 'START') {
                this.connSM.headerReceived();
            }

            if (utils.bufferEquals(serverVersion, constants.amqpVersion)) {
                this.connSM.validVersion();
                this.connSM.sendOpen();
            } else {
                this.connSM.invalidVersion();
            }
            this._receivedHeader = true;
        }

        return true;
    }
    return false;
};

Connection.prototype._tryReceiveFrame = function() {
    try {
        var frame = FrameReader.read(this._curbuf);
        if (frame === undefined) {
            //debug('Not enough frame');
        } else {
            //debug('Matched frame: ' + frame.constructor.name + ': ' + JSON.stringify(frame));
            //debug(this._curbuf.length + ' bytes left');
        }
        return frame;
    } catch (e) {
        console.warn(e.message);
    }
};

Connection.prototype._receiveAny = function() {
    var frame = null;
    var more = true;
    while (more) {
        if (this.sasl) {
            if (!this.sasl.receivedHeader) {
                more = this._tryReceiveHeader(constants.saslVersion);
                continue;
            }
            frame = this._tryReceiveFrame();
            if (frame) {
                this.emit(Connection.FrameReceived, frame);
            } else more = false;
        } else {
            if (!this._receivedHeader) {
                more = this._tryReceiveHeader();
                continue;
            }
            frame = this._tryReceiveFrame();
            if (frame) {
                if (frame instanceof OpenFrame) {
                    this._processOpenFrame(frame);
                } else if (frame instanceof CloseFrame) {
                    this._processCloseFrame(frame);
                    more = false;
                } else {
                    this.emit(Connection.FrameReceived, frame);
                }
            } else more = false;
        }
    }
};

Connection.prototype._ensureLocaleCompatibility = function(lhsLocales, rhsLocales) {
    return true;
};

Connection.prototype._sendOpenFrame = function() {
    this.sendFrame(new OpenFrame(this.connectionParams));
};

Connection.prototype._processOpenFrame = function(frame) {
    this.connSM.openReceived();
    if (this.connSM.getMachineState() === 'OPEN_RCVD') {
        this.connSM.sendOpen();
    }

    var valid = true;
    this.connectionParams.maxFrameSize = Math.min(this.connectionParams.maxFrameSize, frame.maxFrameSize || constants.defaultMaxFrameSize);
    this.connectionParams.channelMax = Math.min(this.connectionParams.channelMax, frame.channelMax || constants.defaultChannelMax);
    this.idleTimeout = Math.min(this.connectionParams.idleTimeout, frame.idleTimeout || constants.defaultIdleTimeout);
    if (frame.outgoingLocales) {
        valid = valid && this._ensureLocaleCompatibility(this.connectionParams.incomingLocales, frame.outgoingLocales);
    }
    if (frame.incomingLocales) {
        valid = valid && this._ensureLocaleCompatibility(this.connectionParams.outgoingLocales, frame.incomingLocales);
    }

    if (!valid) {
        this.connSM.invalidOptions();
    } else {
        // We're connected.
        debug('Connected with params { maxFrameSize='+this.connectionParams.maxFrameSize+', channelMax='+this.connectionParams.channelMax+', idleTimeout='+this.connectionParams.idleTimeout+' }');
        this.connected = true;
        this._lastOutgoing = Date.now();
        var maxTimeBeforeHeartbeat = Math.floor(this.connectionParams.idleTimeout / 2);
        var timeBetweenHeartbeatChecks = Math.floor(this.connectionParams.idleTimeout / 8);
        debug('Setting heartbeat check timeout to ' + timeBetweenHeartbeatChecks);
        var self = this;
        setInterval(function () {
            if (self.connected && (Date.now() - self._lastOutgoing) > maxTimeBeforeHeartbeat) {
                self.sendFrame(new HeartbeatFrame());
            }
        }, timeBetweenHeartbeatChecks);
        this.emit(Connection.Connected, this);
        if (this._curbuf.length) this._receiveAny(); // Might have more frames pending.
    }
};

Connection.prototype._sendCloseFrame = function(err) {
    this.sendFrame(new CloseFrame(err));
};

Connection.prototype._processCloseFrame = function(frame) {
    this.connSM.closeReceived();
    this.connSM.sendClose();
    if (frame.error) {
        this.emit(Connection.ErrorReceived, frame.error);
    }
};

Connection.prototype._connect = function(address, sasl) {
    this.address = address;
    var self = this;
    self.dataHandler = self._receiveHeader;
    var isSSL = address.protocol === 'amqps';
    if (isSSL) {
        var sslOptions = self._sslOptions || {};
        sslOptions.port = self.address.port;
        sslOptions.host = self.address.host;
        self.client = tls.connect(sslOptions);
        debug('Connecting to '+self.address.host+':'+self.address.port+' via TLS');
    } else {
        self.client = net.connect({port: self.address.port, host: self.address.host});
        debug('Connecting to '+self.address.host+':'+self.address.port+' via straight-up sockets');
    }
    self.client.on(isSSL ? 'secureConnect' : 'connect', function() {
        self.connSM.connected(sasl);
    }).on('data', function(buf) {
        self._receiveData(buf);
    }).on('error', function(err) {
        self.connSM.error(err);
    }).on('end', function() {
        self.connSM.terminated();
    });
};

Connection.prototype._saslComplete = function(err) {
    this.sasl = null;
    if (err) {
        this.connSM.error(err);
    } else {
        this.connSM.success();
    }
};

Connection.prototype._terminate = function() {
    if (this.client) {
        this._sslOptions = null;
        this.sasl = null;
        this.address = null;
        this.client.end();
        this.client = null;
    }
    this.connected = false;
    this.emit(Connection.Disconnected, this);
};

module.exports = Connection;
