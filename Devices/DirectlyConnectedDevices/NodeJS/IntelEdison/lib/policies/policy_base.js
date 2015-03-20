var constants   = require('../constants'),
    putils      = require('./policy_utilities');

function containerName() {
    return function() {
        return 'conn' + Date.now();
    };
}

function linkName(prefix) {
    var id = 1;
    var pre = prefix;
    return function() {
        return pre + (id++);
    };
}

/**
 * Policies encode many of the optional behaviors and settings of AMQP into a cohesive place,
 * that could potentially be standardized, could be loaded from JSON, etc.
 */
module.exports = {
    connectPolicy: {
        options: {
            containerId: containerName(),
            hostname: 'localhost',
            maxFrameSize: constants.defaultMaxFrameSize,
            channelMax: constants.defaultChannelMax,
            idleTimeout: constants.defaultIdleTimeout,
            outgoingLocales: constants.defaultOutgoingLocales,
            incomingLocales: constants.defaultIncomingLocales,
            offeredCapabilities: null,
            desiredCapabilities: null,
            properties: {},
            sslOptions: {
                keyFile: null,
                certFile: null,
                caFile: null,
                rejectUnauthorized: false
            }
        }
    },
    sessionPolicy: {
        options: {
            nextOutgoingId: constants.session.defaultOutgoingId,
            incomingWindow: constants.session.defaultIncomingWindow,
            outgoingWindow: constants.session.defaultOutgoingWindow
        },
        windowPolicy: putils.WindowPolicies.RefreshAtHalf,
        windowQuantum: constants.session.defaultIncomingWindow
    },
    senderLinkPolicy: {
        options: {
            name: linkName('sender'),
            role: constants.linkRole.sender,
            senderSettleMode: constants.senderSettleMode.mixed,
            maxMessageSize: 0,
            initialDeliveryCount: 1
        },
        callbackPolicy: putils.SenderCallbackPolicies.OnSettle,
        encoder: function(body) { return body; }
    },
    receiverLinkPolicy: {
        options: {
            name: linkName('receiver'),
            role: constants.linkRole.receiver,
            receiverSettleMode: constants.receiverSettleMode.autoSettle,
            maxMessageSize: 10000 // Arbitrary choice
        },
        creditPolicy: putils.CreditPolicies.RefreshAtHalf,
        creditQuantum: 100,
        decoder: function(body) { return body; }
    }
};
