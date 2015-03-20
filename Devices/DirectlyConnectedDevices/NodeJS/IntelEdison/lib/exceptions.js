var util        = require('util');

/**
 * AMQP Header is malformed.
 *
 * @param msg
 * @constructor
 */
function MalformedHeaderError(msg) {
    this.message = 'Malformed header: ' + msg;
    Error.captureStackTrace(this);
}

util.inherits(MalformedHeaderError, Error);

module.exports.MalformedHeaderError = MalformedHeaderError;

/**
 * Method or feature is not yet implemented.
 *
 * @param msg
 * @constructor
 */
function NotImplementedError(msg) {
    this.message = msg + ' not yet implemented';
    Error.captureStackTrace(this);
}

util.inherits(NotImplementedError, Error);

module.exports.NotImplementedError = NotImplementedError;

/**
 * Payload is malformed or cannot be parsed.
 *
 * @param msg
 * @constructor
 */
function MalformedPayloadError(msg) {
    this.message = 'Malformed payload: ' + msg;
    Error.captureStackTrace(this);
}

util.inherits(MalformedPayloadError, Error);

module.exports.MalformedPayloadError = MalformedPayloadError;

/**
 * Given object cannot be encoded successfully.
 *
 * @param msg
 * @constructor
 */
function EncodingError(msg) {
    this.message = 'Encoding failure: ' + msg;
    Error.captureStackTrace(this);
}

util.inherits(EncodingError, Error);

module.exports.EncodingError = EncodingError;

/**
 * Violation of AMQP flow control.
 *
 * @param msg
 * @constructor
 */
function OverCapacityError(msg) {
    this.message = msg;
    Error.captureStackTrace(this);
}

util.inherits(OverCapacityError, Error);

module.exports.OverCapacityError = OverCapacityError;

/**
 * Authentication failure.
 *
 * @param msg
 * @constructor
 */
function AuthenticationError(msg) {
    this.message = msg;
    Error.captureStackTrace(this);
}

util.inherits(AuthenticationError, Error);

module.exports.AuthenticationError = AuthenticationError;

/**
 * Argument missing or incorrectly defined.
 *
 * @param arg
 * @constructor
 */
function ArgumentError(arg) {
    this.message = (arg instanceof Array) ? 'Must provide arguments ' + arg.join(', ') : 'Must provide argument ' + arg;
    Error.captureStackTrace(this);
}

util.inherits(ArgumentError, Error);

module.exports.ArgumentError = ArgumentError;

/**
 * Convenience method to assert that a given options object contains the required arguments.
 *
 * @param options
 * @param argnames
 */
function assertArguments(options, argnames) {
    if (!argnames) return;
    if (!options) throw new ArgumentError(argnames);
    for (var idx in argnames) {
        var argname = argnames[idx];
        if (!options.hasOwnProperty(argname)) {
            throw new ArgumentError(argname);
        }
    }
}

module.exports.assertArguments = assertArguments;

function assertArgument(arg, argname) {
    if (arg === undefined) throw new ArgumentError(argname);
}

module.exports.assertArgument = assertArgument;

/**
 * Invalid state.
 *
 * @param msg
 * @constructor
 */
function InvalidStateError(msg) {
    this.message = msg;
    Error.captureStackTrace(this);
}

util.inherits(InvalidStateError, Error);

module.exports.InvalidStateError = InvalidStateError;

