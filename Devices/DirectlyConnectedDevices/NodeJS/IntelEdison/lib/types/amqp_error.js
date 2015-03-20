var Int64           = require('node-int64'),
    util            = require('util'),

    DescribedType   = require('./described_type'),
    Symbol          = require('./symbol');

function AMQPError(condition, description, info) {
    AMQPError.super_.call(this, AMQPError.Descriptor.code);
    this.condition = condition;
    this.description = description;
    this.errorInfo = info;
}

util.inherits(AMQPError, DescribedType);

AMQPError.prototype.getValue = function() {
    return [ this.condition, this.description || '', this.errorInfo || '' ];
};

AMQPError.Descriptor = {
    name: new Symbol('amqp:error:list'),
    code: new Int64(0x0, 0x1D)
};

AMQPError.InternalError = new Symbol('amqp:internal-error');
AMQPError.NotFound = new Symbol('amqp:not-found');
AMQPError.UnauthorizedAccess = new Symbol('amqp:unauthorized-access');
AMQPError.DecodeError = new Symbol('amqp:decode-error');
AMQPError.ResourceLimitExceeded = new Symbol('amqp:resource-limit-exceeded');
AMQPError.NotAllowed = new Symbol('amqp:not-allowed');
AMQPError.InvalidField = new Symbol('amqp:invalid-field');
AMQPError.NotImplemented = new Symbol('amqp:not-implemented');
AMQPError.ResourceLocked = new Symbol('amqp:resource-locked');
AMQPError.PreconditionFailed = new Symbol('amqp:precondition-failed');
AMQPError.ResourceDeleted = new Symbol('amqp:resource-deleted');
AMQPError.IllegalState = new Symbol('amqp:illegal-state');
AMQPError.FrameSizeTooSmall = new Symbol('amqp:frame-size-too-small');

// Connection errors
AMQPError.ConnectionForced = new Symbol('amqp:connection:forced');
AMQPError.ConnectionFramingError = new Symbol('amqp:connection:framing-error');
AMQPError.ConnectionRedirect = new Symbol('amqp:connection:redirect');

// Session errors
AMQPError.SessionWindowViolation = new Symbol('amqp:session:window-violation');
AMQPError.SessionErrantLink = new Symbol('amqp:session:errant-link');
AMQPError.SessionHandleInUse = new Symbol('amqp:session:handle-in-use');
AMQPError.SessionUnattachedHandle = new Symbol('amqp:session:unattached-handle');

// Link errors
AMQPError.LinkDetachForced = new Symbol('amqp:link:detach-forced');
AMQPError.LinkTransferLimitExceeded = new Symbol('amqp:link:transfer-limit-exceeded');
AMQPError.LinkMessageSizeExceeded = new Symbol('amqp:link:message-size-exceeded');
AMQPError.LinkRedirect = new Symbol('amqp:link:redirect');
AMQPError.LinkStolen = new Symbol('amqp:link:stolen');

AMQPError.fromDescribedType = function(describedType) {
    return new AMQPError(describedType.value[0], describedType.value[1], describedType.value[2]);
};

module.exports = AMQPError;
