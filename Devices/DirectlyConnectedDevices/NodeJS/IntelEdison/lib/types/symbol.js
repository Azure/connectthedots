/**
 * Encoding for AMQP Symbol type, to differentiate from strings.  More terse than ForcedType.
 *
 * @param {String} str  Symbol contents
 * @constructor
 */
var Symbol = function(str) {
    this.contents = str;
};

Symbol.prototype.toString = function() {
    return this.contents;
};

Symbol.prototype.getValue = function() {
    return ['symbol', this.contents];
};

Symbol.prototype.encode = function(codec, bufb) {
    var asBuf = new Buffer(this.contents, 'utf8');
    if (asBuf.length > 0xFE) {
        bufb.appendUInt8(0xb3);
        bufb.appendUInt32BE(asBuf.length);
    } else {
        bufb.appendUInt8(0xa3);
        bufb.appendUInt8(asBuf.length);
    }
    bufb.appendBuffer(asBuf);
};

Symbol.stringify = function(arrOrSym) {
    if (arrOrSym instanceof Array) {
        var result = [];
        for (var idx in arrOrSym) {
            result.push(arrOrSym[idx].contents);
        }
        return result;
    } else return arrOrSym.contents;
};

module.exports = Symbol;
