#pragma once 

#ifndef _AMQP_JSONMESSAGE_
#define _AMQP_JSONMESSAGE_

#include "..\JSON.h"
#include "TextMessage.h"

namespace amqp
{

	class JsonMessage : public TextMessage
	{
	public:
		JsonMessage(std::string const &subject, JSONObject const &jsonData,
			IMessage::etEncoding encoding = UTF8);
		virtual ~JsonMessage();
	private:
	};

};

#endif // _AMQP_JSONMESSAGE_