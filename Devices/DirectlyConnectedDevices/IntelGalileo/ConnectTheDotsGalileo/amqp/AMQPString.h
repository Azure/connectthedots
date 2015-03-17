#pragma once

#ifndef _AMQP_AMQPSTRING_H_
#define _AMQP_AMQPSTRING_H_

#include <string>
#include "IAMQPData.h"

namespace amqp
{
	class AMQPString : public IAMQPData
	{
	public:
		AMQPString(std::string const &str);
		virtual ~AMQPString() 
		{
		};
		pn_bytes_t getString() const;
		virtual Type getDataType() const;
		virtual std::shared_ptr<IAMQPData> copy() const;
	private:
		std::string m_str;
	};
};

#endif // _AMQP_AMQPSYMBOL_H_