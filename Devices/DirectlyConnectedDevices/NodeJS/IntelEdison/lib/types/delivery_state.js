var Int64           = require('node-int64'),
    util            = require('util'),

    constants       = require('../constants'),
    exceptions      = require('../exceptions'),

    AMQPError       = require('./amqp_error'),
    DescribedType   = require('./described_type'),
    ForcedType      = require('./forced_type'),
    Symbol          = require('./symbol');

function DeliveryState(code) {
    DeliveryState.super_.call(this, code);
}

util.inherits(DeliveryState, DescribedType);

module.exports.DeliveryState = DeliveryState;

function Received(options) {
    Received.super_.call(this, Received.Descriptor.code);
    exceptions.assertArguments(options, [ 'sectionNumber', 'sectionOffset' ]);
    this.sectionNumber = options.sectionNumber;
    this.sectionOffset = options.sectionOffset;
}

util.inherits(Received, DeliveryState);

Received.fromDescribedType = function(describedType) {
    var options = {
        sectionNumber: describedType.value[0],
        sectionOffset: describedType.value[1]
    };
    return new Received(options);
};

Received.prototype.getValue = function() {
    var self = this;
    return [
        new ForcedType('uint', self.sectionNumber),
        new ForcedType('ulong', self.sectionOffset) ];
};

Received.Descriptor = {
    name: new Symbol('amqp:received:list'),
    code: new Int64(0x0, 0x23)
};

module.exports.Received = Received;

function Accepted(options) {
    Accepted.super_.call(this, Accepted.Descriptor.code);
}

util.inherits(Accepted, DeliveryState);

Accepted.fromDescribedType = function(describedType) {
    return new Accepted();
};

Accepted.prototype.getValue = function() {
    return undefined; // Accepted has no fields
};

Accepted.Descriptor = {
    name: new Symbol('amqp:accepted:list'),
    code: new Int64(0x0, 0x24)
};

module.exports.Accepted = Accepted;

function Rejected(options) {
    Rejected.super_.call(this, Rejected.Descriptor.code);
    if (options instanceof AMQPError) {
        this.error = options;
    } else {
        this.error = options.error;
    }
}

util.inherits(Rejected, DeliveryState);

Rejected.fromDescribedType = function(describedType) {
    return new Rejected(describedType.value ? describedType.value[0] : null);
};

Rejected.prototype.getValue = function() {
    return [ this.error || null ];
};

Rejected.Descriptor = {
    name: new Symbol('amqp:rejected:list'),
    code: new Int64(0x0, 0x25)
};

module.exports.Rejected = Rejected;

function Released(options) {
    Released.super_.call(this, Released.Descriptor.code);
}

util.inherits(Released, DeliveryState);

Released.fromDescribedType = function(describedType) {
    return new Released();
};

Released.prototype.getValue = function() {
    return undefined; // Released has no fields
};

Released.Descriptor = {
    name: new Symbol('amqp:released:list'),
    code: new Int64(0x0, 0x26)
};

module.exports.Released = Released;
