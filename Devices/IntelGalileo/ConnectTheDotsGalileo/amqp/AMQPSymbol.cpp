#include "AMQPSymbol.h"

amqp::AMQPSymbol::AMQPSymbol(std::string const &symbol) 
	: m_symbol(symbol)
{

}

pn_bytes_t amqp::AMQPSymbol::getSymbol() const
{
	return pn_bytes(m_symbol.length(), m_symbol.c_str());
}

amqp::IAMQPData::Type amqp::AMQPSymbol::getDataType() const
{
	return (IAMQPData::AMQP_SYMBOL);
}

std::shared_ptr<amqp::IAMQPData> amqp::AMQPSymbol::copy() const
{
	return std::shared_ptr<IAMQPData>(new AMQPSymbol(m_symbol));
}