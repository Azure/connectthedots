var builder     = require('buffer-builder'),

    Symbol      = require('./symbol');

/**
 * Encoding for AMQP Arrays - homogeneous typed collections.  Provides the CODE for the element type.
 *
 * @param {Array} arr           Array contents, should be encode-able to the given code type.
 * @param {Number} elementType  BYTE code-point for the array values (e.g. 0xA1).
 * @constructor
 */
function AMQPArray(arr, elementType) {
    this.array = arr;
    this.elementType = elementType;
}

module.exports.Array = AMQPArray;

function AMQPFields(fields) {
    for (var k in fields) {
        if (fields.hasOwnProperty(k)) {
            this[k] = fields[k];
        }
    }
}

AMQPFields.prototype.encode = function(codec, bufb) {
    var tempBufb = new builder();
    var count = 0;
    for (var k in this) {
        if (this.hasOwnProperty(k)) {
            var v = this[k];
            codec.encode(new Symbol(k), tempBufb);
            codec.encode(v, tempBufb);
            count += 2;
        }
    }
    var tempBuf = tempBufb.get();
    if (tempBuf.length > 0xFE) {
        bufb.appendUInt8(0xD1);
        bufb.appendUInt32BE(tempBuf.length + 4);
        bufb.appendUInt32BE(count);
    } else {
        bufb.appendUInt8(0xC1);
        bufb.appendUInt8(tempBuf.length + 1);
        bufb.appendUInt8(count);
    }
    bufb.appendBuffer(tempBuf);
};

module.exports.Fields = AMQPFields;
