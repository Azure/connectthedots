#pragma once

#ifndef _AMQP_IMESSAGE_H_
#define _AMQP_IMESSAGE_H_

#include <list>
#include <string>
#include <memory>
#include <proton\message.h>
#include "IAMQPData.h"

namespace amqp {

	/// meta information type for key-value AMQP message sections as Annotations and Properties
	typedef std::list<std::shared_ptr<IAMQPData>> MessageMeta;

	/// AMQP Message interface
	class IMessage {
	public:
		/// encoding types
		enum etEncoding { UTF8, UTF16 };

		/// method adds an annotation for AMQP header
		virtual void addAnnotation(IAMQPData const &key, IAMQPData const &value) = 0;

		/// method adds a property for AMQP header
		virtual void addProperty(IAMQPData const &key, IAMQPData const &value) = 0;

		/// method returns Content Type string of the message
		virtual std::string const &getContentType() const = 0;

		/// method returns data bytes of the message 
		virtual char const *getBytes() const = 0;

		/// method returns the size of the message in bytes
		virtual size_t getSize() const = 0;

		/// method returns the subject of the message
		virtual std::string const &getSubject() const = 0;

		/// method returns Properties
		virtual MessageMeta const &getProperties() const = 0;

		/// method returns Annotations
		virtual MessageMeta const &getAnnotations() const = 0;

		/// method returns content encoding
		virtual etEncoding getEncoding() const = 0;

		virtual ~IMessage()
		{
		};
	protected:
		IMessage() 
		{
		};
	private:
	};
};

#endif // _AMQP_IMESSAGE_H_