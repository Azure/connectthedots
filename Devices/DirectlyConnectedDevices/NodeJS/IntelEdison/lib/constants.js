var Int64   = require('node-int64'),
    builder = require('buffer-builder');

function amqpify(arr) {
    var b = new builder();
    b.appendString('AMQP');
    for (var idx in arr) {
        b.appendUInt8(arr[idx]);
    }

    return b.get();
}

var constants = {
    defaultPort: 5672,
    defaultTlsPort: 5671,
    minMaxFrameSize: 512,
    defaultMaxFrameSize: 4294967295,
    defaultChannelMax: 65535,
    defaultIdleTimeout: 120000,
    requiredLocale: 'en-US',
    defaultOutgoingLocales: 'en-US',
    defaultIncomingLocales: 'en-US',
    defaultHandleMax: 4294967295,
    amqpVersion: amqpify([0, 1, 0, 0]),
    saslVersion: amqpify([3, 1, 0, 0]),
    session: {
        defaultIncomingWindow: 100,
        defaultOutgoingWindow: 100,
        defaultOutgoingId: 1
    },
    frameType: {
        amqp: 0x0, sasl: 0x1
    },
    saslOutcomes: {
        ok: 0,
        auth: 1,
        sys: 2,
        sys_perm: 3,
        sys_temp: 4,
        0: 'OK',
        1: 'Authentication failed due to issue with credentials',
        2: 'Authentication failed due to a system error',
        3: 'Authentication failed due to a permanent system error',
        4: 'Authentication failed due to a transient system error'
    },
    linkRole: {
        sender: false,
        receiver: true
    },
    senderSettleMode: {
        unsettled: 0,
        settled: 1,
        mixed: 2
    },
    receiverSettleMode: {
        autoSettle: 0,
        settleOnDisposition: 1
    },
    terminusDurability: {
        none: 0,
        configuration: 1,
        unsettledState: 2
    },
    terminusExpiryPolicy: {
        linkDetach: 'link-detach',
        sessionEnd: 'session-end',
        connectionClose: 'connection-close',
        never: 'never'
    },
    distributionMode: {
        move: 'move',
        copy: 'copy'
    }
};

module.exports = constants;
