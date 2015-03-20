var debug       = require('debug')('amqp10-DescribedType'),

    exceptions  = require('./../exceptions');

/**
 * Described type, as described in the AMQP 1.0 spec as follows:
 *
<pre>
             constructor                       untyped bytes
                  |                                 |
      +-----------+-----------+   +-----------------+-----------------+
      |                       |   |                                   |
 ...  0x00 0xA1 0x03 "URL" 0xA1   0x1E "http://example.org/hello-world"  ...
           |             |  |     |                                   |
           +------+------+  |     |                                   |
                  |         |     |                                   |
             descriptor     |     +------------------+----------------+
                            |                        |
                            |         string value encoded according
                            |             to the str8-utf8 encoding
                            |
                 primitive format code
               for the str8-utf8 encoding

</pre>
 *
 * (Note: this example shows a string-typed descriptor, which should be considered reserved)
 *
 * @constructor
 * @param descriptor        Descriptor for the type (can be any valid AMQP type, including another described type).
 * @param value             Value of the described type (can also be any valid AMQP type, including another described type).
 */
function DescribedType(descriptor, value) {
    this.descriptor = descriptor;
    this.value = value;
}

DescribedType.prototype.getValue = function() {
    return this.value;
};

module.exports = DescribedType;
