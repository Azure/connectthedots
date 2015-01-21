#include "Sender.h"

amqp::Sender::Sender()
{
	m_messenger = pn_messenger(NULL);
	_startMessenger();
}

amqp::Sender::Sender(std::string const &name) :
		m_name(name)
{
	m_messenger = pn_messenger(m_name.c_str());
	_startMessenger();
}

amqp::Sender::~Sender()
{
	if (m_messenger != NULL) {
		pn_messenger_stop(m_messenger);
		pn_messenger_free(m_messenger);
	}
}

void amqp::Sender::_startMessenger()
{
	pn_messenger_set_outgoing_window(m_messenger, 5);
	pn_messenger_start(m_messenger);
}

void amqp::Sender::send(IMessage const &message, Address const &address)
{
	pn_data_t *pnmessage_body = NULL;
	pn_message_t *pnmessage = pn_message();

	if (pnmessage == NULL) {
		throw std::exception("ERROR: Message could not be created.");
	}

	pn_message_set_address(pnmessage, address.toString().c_str());
	_addMetaToMessage(pnmessage, message);

	pnmessage_body = pn_message_body(pnmessage);
	pn_data_put_binary(pnmessage_body, pn_bytes(message.getSize(), message.getBytes()));
	pn_messenger_put(m_messenger, pnmessage);

	if (isError()) {
		throw std::exception(pn_error_text(pn_messenger_error(m_messenger)));
	}

	pn_messenger_send(m_messenger, 1);

	if (isError()) {
		throw std::exception(pn_error_text(pn_messenger_error(m_messenger)));
	}

	pn_message_free(pnmessage);
}

void amqp::Sender::_addMetaToMessage(pn_message_t *pnmessage, IMessage const &message)
{
	pn_timestamp_t utcTime;
	time(&utcTime);

	_addPropertiesToMessage(pnmessage, message);
	_addAnnotationsToMessage(pnmessage, message);

	pn_message_set_content_type(pnmessage, message.getContentType().c_str());
	pn_message_set_inferred(pnmessage, true);
	pn_message_set_subject(pnmessage, message.getSubject().c_str());
	pn_message_set_ttl(pnmessage, 86400000);
	pn_message_set_creation_time(pnmessage, utcTime);
	switch (message.getEncoding())
	{
	case IMessage::UTF8: 
		pn_message_set_content_encoding(pnmessage, "UTF-8");
		break;
	case IMessage::UTF16:
		pn_message_set_content_encoding(pnmessage, "UTF-16");
		break;
	}
	
}

void amqp::Sender::_addPropertiesToMessage(pn_message_t *pnmessage, IMessage const &message)
{
	pn_data_t *pnmessage_properties = pn_message_properties(pnmessage);
	pn_data_put_map(pnmessage_properties);
	pn_data_enter(pnmessage_properties);

	MessageMeta::const_iterator i = message.getProperties().begin();
	for (; i != message.getProperties().end(); ++i) {
		_serializeMessageMeta(pnmessage_properties, (*i));
	}

	pn_data_exit(pnmessage_properties);
}

void amqp::Sender::_addAnnotationsToMessage(pn_message_t *pnmessage, IMessage const &message)
{
	pn_data_t *pnmessage_annotations = pn_message_annotations(pnmessage);
	pn_data_put_map(pnmessage_annotations);
	pn_data_enter(pnmessage_annotations);

	MessageMeta::const_iterator i = message.getAnnotations().begin();
	for (; i != message.getAnnotations().end(); ++i) {
		_serializeMessageMeta(pnmessage_annotations, (*i));
	}

	pn_data_exit(pnmessage_annotations);
}

void amqp::Sender::_serializeMessageMeta(pn_data_t *pndata, std::shared_ptr<amqp::IAMQPData> const &data)
{
		switch (data->getDataType())
		{
		case IAMQPData::AMQP_STRING: 
			pn_data_put_string(pndata, (std::dynamic_pointer_cast<AMQPString const>(data))->getString());
			break;
		case IAMQPData::AMQP_SYMBOL:
			pn_data_put_symbol(pndata, (std::dynamic_pointer_cast<AMQPSymbol const>(data))->getSymbol());
			break;
		case IAMQPData::AMQP_UUID:
			pn_data_put_uuid(pndata, (std::dynamic_pointer_cast<AMQPuuid const>(data))->getUUID());
			break;
		default:
			throw std::exception("ERROR: wrong data type");
		}
}

bool amqp::Sender::isError()
{
	return (pn_messenger_errno(m_messenger) != 0);
}

