#include "TextMessage.h"

amqp::TextMessage::TextMessage(std::string const &subject, std::wstring const &message,
	etEncoding encoding,
	std::string const &contentType)
	: m_subject(subject),
	m_contentType(contentType),
	m_encoding(encoding)
{
	setMessage(message, encoding);
}

amqp::TextMessage::TextMessage(std::string const &subject, std::string const &message,
	etEncoding encoding,
	std::string const &contentType)
	: m_subject(subject),
	m_contentType(contentType),
	m_encoding(encoding)
{
	setMessage(message, encoding);
}

amqp::TextMessage::~TextMessage()
{

};

void amqp::TextMessage::addAnnotation(IAMQPData const &key, IAMQPData const &value)
{
	m_annotations.push_back(key.copy());
	m_annotations.push_back(value.copy());

}

void amqp::TextMessage::addProperty(IAMQPData const &key, IAMQPData const &value)
{
	m_properties.push_back(key.copy());
	m_properties.push_back(value.copy());
}

std::string const &amqp::TextMessage::getContentType() const
{
	return (m_contentType);
}

char const *amqp::TextMessage::getBytes() const
{
	if (m_encoding == IMessage::UTF8) {
		return (m_message.c_str());
	}
	else {
		return (reinterpret_cast<char const *>(m_messageW.c_str()));
	}
}

size_t amqp::TextMessage::getSize() const
{
	if (m_encoding == IMessage::UTF8) {
		return (m_message.length());
	}
	else {
		return (m_messageW.length() * sizeof(wchar_t));
	}
}

amqp::IMessage::etEncoding amqp::TextMessage::getEncoding() const
{
	return (m_encoding);
}

amqp::MessageMeta const &amqp::TextMessage::getProperties() const
{
	return (m_properties);
}

amqp::MessageMeta const &amqp::TextMessage::getAnnotations() const
{
	return (m_annotations);
}

std::string const &amqp::TextMessage::getSubject() const
{
	return (m_subject);
}

void amqp::TextMessage::setContentType(std::string const &contentType)
{
	m_contentType = contentType;
}

void amqp::TextMessage::setSubject(std::string const &subject)
{
	m_subject = subject;
}

void amqp::TextMessage::setMessage(std::wstring const &message,
	etEncoding encoding)
{
	m_encoding = encoding;
	m_messageW = message;
	if (m_encoding == IMessage::UTF8) {
		std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> conv;
		m_message = conv.to_bytes(m_messageW);
	}
}

void amqp::TextMessage::setMessage(std::string const &message,
	etEncoding encoding)
{
	m_encoding = encoding;
	m_message = message;
	if (m_encoding == IMessage::UTF16) {
		std::wstring_convert< std::codecvt<wchar_t, char, std::mbstate_t> > conv;
		m_messageW = conv.from_bytes(m_message);
	}
}