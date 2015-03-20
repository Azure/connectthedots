var debug           = require('debug')('amqp10-KnownTypeConverter'),

    DescribedType   = require('./described_type'),

    AMQPError       = require('./amqp_error'),
    DeliveryStates  = require('./delivery_state'),
    Source          = require('./source_target').Source,
    Target          = require('./source_target').Target;
    Message         = require('./message');

function convertType(describedType) {
    var knownTypes = [
        AMQPError, Source, Target, DeliveryStates.Accepted, DeliveryStates.Received, DeliveryStates.Rejected, DeliveryStates.Released,
        Message.Header, Message.DeliveryAnnotations, Message.Annotations, Message.Properties, Message.ApplicationProperties,
        Message.Footer, Message.Data, Message.AMQPSequence, Message.AMQPValue
    ];
    var descriptorStr = describedType.descriptor.toString();
    for (var idx in knownTypes) {
        var curType = knownTypes[idx];
        if (curType.Descriptor.name.toString() == descriptorStr || curType.Descriptor.code.toString() == descriptorStr) {
            return curType.fromDescribedType(describedType);
        }
    }

    return undefined;
}

module.exports = convertType;