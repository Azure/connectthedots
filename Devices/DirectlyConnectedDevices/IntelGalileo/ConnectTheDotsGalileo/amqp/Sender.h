#pragma once

#ifndef _AMQP_SENDER_
#define _AMQP_SENDER_

#include <string>
#include <time.h>
#include <proton\messenger.h>
#include <proton\types.h>
#include <memory>
#include <locale>
#include <arduino.h>
#include "IMessage.h"
#include "Address.h"
#include "AMQPData.h"

namespace amqp {

	class Sender {
	public:
		Sender(bool trackMessages = false);
		Sender(std::string const &name, bool trackMessages = false);
		virtual ~Sender();

		/// method sends message to AMQP broker
		void send(IMessage const &message, Address const &address);

		/// method returns true if there is an error
		bool isError() const;

		/// method returns true if tracking was enabled
		bool isTraking() const;
		
	private:
		pn_messenger_t *m_messenger = NULL;
		pn_tracker_t m_tracker;
		std::string m_name;
		bool m_trackMessages;

		/// method starts messenger 
		void _startMessenger();

		/// method formats message properties
		void _addPropertiesToMessage(pn_message_t *pnmessage, IMessage const &message);

		/// method formats message annotations
		void _addAnnotationsToMessage(pn_message_t *pnmessage, IMessage const &message);

		/// method adds meta information to the message header
		void _addMetaToMessage(pn_message_t *pnmessage, IMessage const &message);
		
		/// method serializes message data
		void _serializeMessageMeta(pn_data_t *pndata, std::shared_ptr<IAMQPData> const &data);
		
		/// method verifies the message status
		void _checkTracking();

		/// method throws an error with text details
		void _throwError();
	};

};

#endif // _AMQP_SENDER_