#include "JsonMessage.h"

amqp::JsonMessage::JsonMessage(std::string const &subject, JSONObject const &jsonData,
	amqp::IMessage::etEncoding encoding)
	: amqp::TextMessage(subject, L"", encoding, "text/json")
{
	JSONValue value(jsonData);
	setMessage(value.Stringify(), encoding);
}

amqp::JsonMessage::~JsonMessage()
{

}
