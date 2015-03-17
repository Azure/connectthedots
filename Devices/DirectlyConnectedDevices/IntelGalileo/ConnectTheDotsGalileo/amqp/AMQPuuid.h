#pragma once

#ifndef _AMQP_AMQPUUID_H_
#define _AMQP_AMQPUUID_H_

#include <string>
#include "IAMQPData.h"

namespace amqp
{
	class AMQPuuid : public IAMQPData
	{
	public:
		AMQPuuid(std::string const &uuid);
		virtual ~AMQPuuid() 
		{
		};
		pn_uuid_t getUUID() const;
		virtual Type getDataType() const;
		virtual std::shared_ptr<IAMQPData> copy() const;
	private:
		std::string m_uuid;
	};
};

#endif // _AMQP_AMQPUUID_H_