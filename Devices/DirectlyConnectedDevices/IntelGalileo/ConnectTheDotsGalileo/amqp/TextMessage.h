#pragma once

#ifndef _AMQP_TEXTMESSAGE_H_
#define _AMQP_TEXTMESSAGE_H_

#include <locale>
#include <codecvt>
#include "IMessage.h"

namespace amqp
{
	class TextMessage : public IMessage {
	public:

		TextMessage(std::string const &subject, std::wstring const &message, 
			etEncoding encoding = UTF16,
			std::string const &contentType = "text/plain");
		TextMessage(std::string const &subject, std::string const &message,
			etEncoding encoding = UTF8,
			std::string const &contentType = "text/plain");
		virtual ~TextMessage();

		virtual void addAnnotation(IAMQPData const &key, IAMQPData const &value);
		virtual void addProperty(IAMQPData const &key, IAMQPData const &value);

		virtual std::string const &getContentType() const;
		virtual char const *getBytes() const;
		virtual size_t getSize() const;
		virtual std::string const &getSubject() const;
		virtual IMessage::etEncoding getEncoding() const;

		virtual MessageMeta const &getProperties() const;
		virtual MessageMeta const &getAnnotations() const;

		virtual void setContentType(std::string const &contentType);
		virtual void setSubject(std::string const &subject);
		virtual void setMessage(std::wstring const &message, 
								etEncoding encoding = UTF16);
		virtual void setMessage(std::string const &message,
								etEncoding encoding = UTF8);

	private:
		MessageMeta m_properties;
		MessageMeta m_annotations;
		std::wstring m_messageW;
		std::string m_message;
		std::string m_subject;
		std::string m_contentType;
		etEncoding m_encoding;
	};
};

#endif // _AMQP_TEXTMESSAGE_H_