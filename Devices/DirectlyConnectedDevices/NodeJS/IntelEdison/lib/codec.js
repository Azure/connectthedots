var debug       = require('debug')('amqp10-Codec'),
    util        = require('util'),
    butils      = require('butils'),
    CBuffer     = require('cbarrick-circular-buffer'),
    builder     = require('buffer-builder'),
    Int64       = require('node-int64'),

    constants   = require('./constants'),

    AMQPArray   = require('./types/amqp_composites').Array,
    DescribedType = require('./types/described_type'),
    ForcedType  = require('./types/forced_type'),
    KnownTypes  = require('./types/known_type_converter'),
    Symbol      = require('./types/symbol'),

    exceptions  = require('./exceptions'),
    types       = require('./types');


/**
 * Build a codec.
 *
 * @constructor
 */
var Codec = function() {
};

Codec.prototype._remaining = function(buf, offset) {
    return buf.length - offset;
};

Codec.prototype._peek = function(buf, offset, numBytes) {
    return (buf instanceof CBuffer) ? buf.peek(numBytes + offset).slice(offset) : buf.slice(offset, offset + numBytes);
};

Codec.prototype._read = function(buf, offset, numBytes) {
    return (buf instanceof CBuffer) ? buf.read(numBytes + offset).slice(offset) : buf.slice(offset, offset + numBytes);
};

/**
 * Reads a full value's worth of bytes from a circular or regular buffer, or returns undefined if not enough bytes are there.
 * Note that for Buffers, the returned Buffer will be a slice (so backed by the original storage)!
 *
 * @param {Buffer|CBuffer} buf              Buffer or circular buffer to read from.  If a Buffer is given, it is assumed to be full.
 * @param {integer} [offset=0]              Offset - only valid for Buffer, not CBuffer.
 * @param {boolean} [doNotConsume=false]    If set to true, will peek bytes instead of reading them - useful for leaving
 *                                          circular buffer in original state for described values that are not yet complete.
 * @param {Number} [forcedCode]             If given, first byte is not assumed to be code and given code will be used - useful for arrays.
 * @returns {Array}                         Buffer of full value + number of bytes read.
 *                                          For described types, will return [ [ descriptor-buffer, value-buffer ], total-bytes ].
 * @private
 */
Codec.prototype._readFullValue = function(buf, offset, doNotConsume, forcedCode) {
    offset = offset || 0;

    var self = this;
    var remaining = this._remaining(buf, offset);
    if (remaining < 1) return undefined;

    var code = forcedCode;
    var codeBytes = 0;
    if (code === undefined) {
        code = this._peek(buf, offset, 1)[0];
        codeBytes = 1;
    }

    // Constructor - need to read two full values back to back of unknown size.  (╯°□°）╯︵ ┻━┻
    if (code === 0x00) {
        var val1 = this._readFullValue(buf, offset + codeBytes, true);
        if (val1 !== undefined) {
            var val2 = this._readFullValue(buf, offset + codeBytes + val1[1], true);
            if (val2 !== undefined) {
                // Now, consume the bytes
                var totalBytes = val1[1] + val2[1] + codeBytes;
                this._read(buf, offset, totalBytes);
                return [ [val1[0], val2[0]], totalBytes];
            }
        }
        return undefined;
    }

    var codePrefix = code & 0xF0;
    var codeAndLength, numBytes;
    var reader = doNotConsume ? self._peek : self._read;
    var readFixed = function(nBytes) { return remaining >= nBytes ? [ reader(buf, offset, nBytes), nBytes ] : undefined; };
    switch (codePrefix) {
        case 0x40: return readFixed(1);
        case 0x50: return readFixed(2);
        case 0x60: return readFixed(3);
        case 0x70: return readFixed(5);
        case 0x80: return readFixed(9);
        case 0x90: return readFixed(17);
        case 0xA0:
        case 0xC0:
        case 0xE0:
            if (remaining < 2) return undefined;
            codeAndLength = this._peek(buf, offset, codeBytes + 1);
            numBytes = codeAndLength[codeBytes] + 1 + codeBytes; // code + size + # octets
            //debug('Reading variable with prefix 0x'+codePrefix.toString(16)+' of length '+numBytes);
            return remaining >= numBytes ? [ reader(buf, offset, numBytes), numBytes ] : undefined;
        case 0xB0:
        case 0xD0:
        case 0xF0:
            if (remaining < 5) return false;
            codeAndLength = this._peek(buf, offset, codeBytes + 4);
            numBytes = butils.readInt32(codeAndLength, codeBytes) + 4 + codeBytes; // code + size + #octets
            //debug('Reading variable with prefix 0x'+codePrefix.toString(16)+' of length '+numBytes);
            return remaining >= numBytes ? [ reader(buf, offset, numBytes), numBytes ] : undefined;

        default:
            throw new exceptions.MalformedPayloadError('Unknown code prefix: 0x' + codePrefix.toString(16));
    }
};

Codec.prototype._decode = function(buf, forcedCode) {
    //debug('Decoding '+buf.toString('hex'));
    var code = forcedCode || buf[0];
    var decoder = types.decoders[code];
    if (!decoder) {
        throw new exceptions.MalformedPayloadError('Unknown code: 0x' + code.toString(16));
    } else {
        var valBytes = forcedCode ? buf : buf.slice(1);
        return decoder(valBytes, this);
    }
};

Codec.prototype._asMostSpecific = function(buf, forcedCode) {
    if (buf instanceof Array) {
        // Described type
        var descriptor = this._asMostSpecific(buf[0]);
        var value = this._asMostSpecific(buf[1]);
        var describedType = new DescribedType(descriptor, value);
        var asKnownType = KnownTypes(describedType);
        return asKnownType || describedType;
    }

    return this._decode(buf, forcedCode);
};

/**
 * Decode a single entity from a buffer (starting at offset 0).  Only simple values currently supported.
 *
 * @param {Buffer|CBuffer} buf          The buffer/circular buffer to decode.  Will decode a single value per call.
 * @param {Number} [offset=0]           The offset to read from (only used for Buffers).
 * @param {Number} [forcedCode]         If given, will not consume first byte for code and will instead use this as the code. Useful for arrays.
 * @return {Array}                      Single decoded value + number of bytes consumed.
 */
Codec.prototype.decode = function(buf, offset, forcedCode) {
    var fullBuf = this._readFullValue(buf, offset, /* doNotConsume= */false, forcedCode);
    if (fullBuf) {
        var bufVal = fullBuf[0];
        var numBytes = fullBuf[1];
        var value = this._asMostSpecific(bufVal, forcedCode);
        return [ value, numBytes ];
    }

    /** @todo debug unmatched */
    return undefined;
};

/**
 * Encode the given value as an AMQP 1.0 bitstring.
 *
 * We do a best-effort to determine type.  Objects will be encoded as <code>maps</code>, unless:
 * + They are DescribedTypes, in which case they will be encoded as such.
 * + They contain an encodeOrdering array, in which case they will be encoded as a <code>list</code> of their values
 *   in the specified order.
 * + They are Int64s, in which case they will be encoded as <code>longs</code>.
 *
 * @param val                           Value to encode.
 * @param {builder} buf                 buffer-builder to write into.
 * @param {string} [forceType]          If set, forces the encoder for the given type.
 */
Codec.prototype.encode = function(val, buf, forceType) {
    var type = typeof val;
    var encoder;
    // Special-case null values
    if (val === null) {
        encoder = types.builders.null;
        return encoder(val, buf);
    }

    switch (type) {
        case 'boolean':
            encoder = types.builders[forceType || 'boolean'];
            return encoder(val, buf);

        case 'string':
            encoder = types.builders[forceType || 'string'];
            return encoder(val, buf);

        case 'number':
            var typeName = 'double';
            if (forceType) {
                typeName = forceType;
            } else {
                /** @todo signed vs. unsigned, byte/short/int32/long */
                if (val % 1 === 0) {
                    if (Math.abs(val) < 0x7FFFFFFF) {
                        typeName = 'int';
                    } else {
                        typeName = 'long';
                    }
                } else {
                    /** @todo float vs. double */
                }
            }
            encoder = types.builders[typeName];
            if (!encoder) throw new exceptions.NotImplementedError('encode(' + typeName + ')');
            return encoder(val, buf);

        case 'object':
            if (val instanceof Buffer) {
                encoder = types.builders.binary;
                return encoder(val, buf, this);
            } else if (val instanceof AMQPArray) {
                encoder = types.builders.array;
                return encoder(val, buf, this);
            } else if (val instanceof Array) {
                encoder = types.builders.list;
                return encoder(val, buf, this);
            } else if (val instanceof ForcedType) {
                return this.encode(val.value, buf, val.typeName);
            } else if (val instanceof DescribedType) {
                buf.appendUInt8(0x00);
                this.encode(val.descriptor, buf);
                this.encode(val.getValue() || [], buf);
                return;
            } else if (val instanceof Int64) {
                encoder = types.builders.ulong;
                if (val < 0) encoder = types.builders.long;
                encoder(val, buf, this);
            } else if (val instanceof Symbol) {
                encoder = types.builders.symbol;
                encoder(val.contents, buf, this);
            } else if (val.encodeOrdering && val.encodeOrdering instanceof Array) {
                // Encoding an object's values in a specific ordering as a list.
                var asList = []; // LINQify
                for (var idx in val.encodeOrdering) {
                    var field = val.encodeOrdering[idx];
                    if (val[field] === undefined) throw new exceptions.EncodingError('Encoding map as list failed, encodeOrdering[' + field + '] field not found or set as undefined in map ' + JSON.stringify(val));
                    asList.push(val[field]);
                }
                return this.encode(asList, buf);
            } else if (val.encode && (typeof val.encode === 'function')) {
                val.encode(this, buf);
            } else {
                return types.builders.map(val, buf, this);
            }
            break;

        default:
            throw new exceptions.NotImplementedError('encode('+type+')');
    }
};

module.exports = new Codec();
