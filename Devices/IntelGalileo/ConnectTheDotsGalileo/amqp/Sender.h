#pragma once

#ifndef _AMQP_SENDER_
#define _AMQP_SENDER_

#include <string>
#include <time.h>
#include <proton\messenger.h>
#include <proton\types.h>
#include <memory>
#include <locale>
#include "IMessage.h"
#include "Address.h"
#include "AMQPData.h"

namespace amqp {

	class Sender {
	public:
		Sender();
		Sender(std::string const &name);
		virtual ~Sender();

		/// send message to AMQP broker
		void send(IMessage const &message, Address const &address);

		/// return true if there is an error
		bool isError();
		
	private:
		pn_messenger_t *m_messenger = NULL;
		std::string m_name;

		void _startMessenger();
		void _addPropertiesToMessage(pn_message_t *pnmessage, IMessage const &message);
		void _addAnnotationsToMessage(pn_message_t *pnmessage, IMessage const &message);
		void _addMetaToMessage(pn_message_t *pnmessage, IMessage const &message);
		void _serializeMessageMeta(pn_data_t *pndata, std::shared_ptr<IAMQPData> const &data);
	};

};

#endif // _AMQP_SENDER_