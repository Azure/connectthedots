var Int64           = require('node-int64'),
    util            = require('util'),

    constants       = require('../constants'),
    exceptions      = require('../exceptions'),
    u               = require('../utilities'),

    AMQPFields      = require('./amqp_composites').Fields,
    AMQPError       = require('./amqp_error'),
    DescribedType   = require('./described_type'),
    ForcedType      = require('./forced_type'),
    Symbol          = require('./symbol');

function Source(options) {
    Source.super_.call(this, Source.Descriptor.code);
    options = options || {};
    this.address = u.orNull(options.address);
    this.durable = u.onUndef(options.durable, constants.terminusDurability.none);
    this.expiryPolicy = u.onUndef(options.expiryPolicy, constants.terminusExpiryPolicy.sessionEnd);
    this.timeout = u.onUndef(options.timeout, 0);
    this.dynamic = u.onUndef(options.dynamic, false);
    this.dynamicNodeProperties = options.dynamicNodeProperties;
    this.distributionMode = options.distributionMode;
    this.filter = options.filter;
    this.defaultOutcome = options.defaultOutcome;
    this.outcomes = options.outcomes;
    this.capabilities = options.capabilities;
}

util.inherits(Source, DescribedType);

Source.Descriptor = {
    name: new Symbol('amqp:source:list'),
    code: new Int64(0x0, 0x28)
};

Source.fromDescribedType = function(describedType) {
    var sourceArr = describedType.value;
    var idx = 0;
    var options = {
        address : u.orNull(sourceArr[idx++]),
        durable: u.onUndef(sourceArr[idx++], constants.terminusDurability.none),
        expiryPolicy: u.onUndef(sourceArr[idx++], constants.terminusExpiryPolicy.sessionEnd),
        timeout: u.onUndef(sourceArr[idx++], 0),
        dynamic: u.onUndef(sourceArr[idx++], false),
        dynamicNodeProperties: sourceArr[idx++],
        distributionMode: sourceArr[idx++],
        filter: sourceArr[idx++],
        defaultOutcome: sourceArr[idx++],
        outcomes: sourceArr[idx++],
        capabilities: sourceArr[idx++]
    };
    return new Source(options);
};

Source.prototype.getValue = function() {
    var self = this;
    return {
        address: self.address,
        durable: new ForcedType('uint', u.onUndef(self.durable, constants.terminusDurability.none)),
        expiryPolicy: u.coerce(u.onUndef(self.expiryPolicy, constants.terminusExpiryPolicy.sessionEnd), Symbol),
        timeout: new ForcedType('uint', u.onUndef(self.timeout, 0)),
        dynamic: u.onUndef(self.dynamic, false),
        dynamicNodeProperties: u.onUndef(self.dynamicNodeProperties, {}),
        distributionMode: u.coerce(self.distributionMode, Symbol),
        filter: u.coerce(u.onUndef(self.filter, {}), AMQPFields),
        defaultOutcome: u.orNull(self.defaultOutcome),
        outcomes: u.orNull(self.outcomes),
        capabilities: u.orNull(self.capabilities),
        encodeOrdering: ['address', 'durable', 'expiryPolicy', 'timeout', 'dynamic', 'dynamicNodeProperties',
            'distributionMode', 'filter', 'defaultOutcome', 'outcomes', 'capabilities']
    };
};

module.exports.Source = Source;

function Target(options) {
    Target.super_.call(this, Target.Descriptor.code);
    options = options || {};
    this.address = u.orNull(options.address);
    this.durable = u.onUndef(options.durable, constants.terminusDurability.none);
    this.expiryPolicy = u.coerce(u.onUndef(options.expiryPolicy, constants.terminusExpiryPolicy.sessionEnd), Symbol);
    this.timeout = u.onUndef(options.timeout, 0);
    this.dynamic = u.onUndef(options.dynamic, false);
    this.dynamicNodeProperties = options.dynamicNodeProperties;
    this.capabilities = options.capabilities;
}

util.inherits(Target, DescribedType);

Target.Descriptor = {
    name: new Symbol('amqp:target:list'),
    code: new Int64(0x0, 0x29)
};

Target.fromDescribedType = function(describedType) {
    var targetArr = describedType.value;
    var idx = 0;
    var options = {
        address: u.orNull(targetArr[idx++]),
        durable: u.onUndef(targetArr[idx++], constants.terminusDurability.none),
        expiryPolicy: u.onUndef(targetArr[idx++], constants.terminusExpiryPolicy.sessionEnd),
        timeout: u.onUndef(targetArr[idx++], 0),
        dynamic: u.onUndef(targetArr[idx++], false),
        dynamicNodeProperties: u.onUndef(targetArr[idx++], {}),
        capabilities: u.orNull(targetArr[idx++])
    };
    return new Target(options);
};

Target.prototype.getValue = function() {
    var self = this;
    return {
        address: u.orNull(self.address),
        durable: new ForcedType('uint', u.onUndef(self.durable, constants.terminusDurability.none)),
        expiryPolicy: u.onUndef(self.expiryPolicy, constants.terminusExpiryPolicy.sessionEnd),
        timeout: new ForcedType('uint', u.onUndef(self.timeout, 0)),
        dynamic: u.onUndef(self.dynamic, false),
        dynamicNodeProperties: u.onUndef(self.dynamicNodeProperties, {}),
        capabilities: u.orNull(self.capabilities),
        encodeOrdering: ['address', 'durable', 'expiryPolicy', 'timeout', 'dynamic', 'dynamicNodeProperties', 'capabilities']
    };
};

module.exports.Target = Target;
