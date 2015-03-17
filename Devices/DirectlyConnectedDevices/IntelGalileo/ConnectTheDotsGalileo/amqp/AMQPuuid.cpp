#include "AMQPuuid.h"

amqp::AMQPuuid::AMQPuuid(std::string const &uuid)
	: m_uuid(uuid)
{
	
}

pn_uuid_t amqp::AMQPuuid::getUUID() const
{
	pn_uuid_t uuid;
	m_uuid._Copy_s(uuid.bytes, sizeof(uuid.bytes), m_uuid.length());
	return (uuid);
}

amqp::IAMQPData::Type amqp::AMQPuuid::getDataType() const
{
	return (IAMQPData::AMQP_UUID);
}

std::shared_ptr<amqp::IAMQPData> amqp::AMQPuuid::copy() const
{
	return std::shared_ptr<IAMQPData>(new AMQPuuid(m_uuid));
}