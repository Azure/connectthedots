var debug = require('debug')('amqp10-PolicyUtils');

var WindowPolicies = {
    RefreshAtHalf : function (session) {
        if (session._sessionParams.incomingWindow < (session.policy.windowQuantum/2)) {
            debug('Refreshing session window by ' + session.policy.windowQuantum + ': ' + session._sessionParams.incomingWindow + ' remaining.');
            session.addWindow(session.policy.windowQuantum);
        }
    },
    RefreshAtEmpty : function (link) {
        if (session._sessionParams.incomingWindow <= 0) {
            debug('Refreshing session window by ' + session.policy.windowQuantum + ': ' + session._sessionParams.incomingWindow + ' remaining.');
            session.addWindow(session.policy.windowQuantum);
        }
    },
    DoNotRefresh : function(link) {
        // Do Nothing
    }
};

module.exports.WindowPolicies = WindowPolicies;

var CreditPolicies = {
    RefreshAtHalf : function (link) {
        if (link.linkCredit < (link.policy.creditQuantum/2)) {
            debug('Refreshing link ' + link.name + ' credit by ' + link.policy.creditQuantum + ': ' + link.linkCredit + ' remaining.');
            link.addCredits(link.policy.creditQuantum);
        }
    },
    RefreshAtEmpty : function (link) {
        if (link.linkCredit <= 0) {
            debug('Refreshing link ' + link.name + ' credit by ' + link.policy.creditQuantum + ': ' + link.linkCredit + ' remaining.');
            link.addCredits(link.policy.creditQuantum);
        }
    },
    DoNotRefresh : function(link) {
        // Do Nothing
    }
};

module.exports.CreditPolicies = CreditPolicies;

var SenderCallbackPolicies = {
    // Only callback when settled Disposition received from recipient
    OnSettle: 'settled',
    // Callback as soon as sent, will not call-back again if future disposition results in error.
    OnSent: 'sent'
};

module.exports.SenderCallbackPolicies = SenderCallbackPolicies;
