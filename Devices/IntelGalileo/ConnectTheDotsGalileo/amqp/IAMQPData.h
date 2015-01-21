#pragma once

#ifndef _AMQP_IAMQPDATA_H_
#define _AMQP_IAMQPDATA_H_

#include <proton\message.h>
#include <proton\types.h>
#include <memory>

namespace amqp
{

	class IAMQPData {
	public:
		enum Type {AMQP_STRING, AMQP_UUID, AMQP_SYMBOL};

		virtual Type getDataType() const = 0;
		virtual std::shared_ptr<IAMQPData> copy() const = 0;

		virtual ~IAMQPData() 
		{
		};
	protected:
		IAMQPData() 
		{
		};
	private:
	};

	class AMQPDataComparator
	{
		virtual bool operator () (IAMQPData const &key1, IAMQPData const &key2) const
		{
			return true;
		}
	};
};

#endif // _AMQP_IAMQPDATA_H_