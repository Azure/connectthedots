#include "AMQPString.h"

amqp::AMQPString::AMQPString(std::string const &str)
	: m_str(str)
{

}

pn_bytes_t amqp::AMQPString::getString() const
{
	return pn_bytes_dup(m_str.length(), m_str.c_str());
}

amqp::IAMQPData::Type amqp::AMQPString::getDataType() const
{
	return (IAMQPData::AMQP_STRING);
}

std::shared_ptr<amqp::IAMQPData> amqp::AMQPString::copy() const
{
	return std::shared_ptr<IAMQPData>(new AMQPString(m_str));
}