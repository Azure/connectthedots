#pragma once

#ifndef _AMQP_AMQPSYMBOL_H_
#define _AMQP_AMQPSYMBOL_H_

#include <string>
#include "IAMQPData.h"

namespace amqp
{
	class AMQPSymbol : public IAMQPData
	{
	public:
		AMQPSymbol(std::string const &symbol);
		virtual ~AMQPSymbol() 
		{
		};
		pn_bytes_t getSymbol() const;
		virtual Type getDataType() const;
		virtual std::shared_ptr<IAMQPData> copy() const;
	private:
		std::string m_symbol;
	};
};

#endif // _AMQP_AMQPSYMBOL_H_