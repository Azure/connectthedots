#include "Address.h"

amqp::Address::Address(std::string const &host,
	std::string const &user,
	std::string const &password,
	std::string const &path,
	int port,
	std::string const &scheme) :
	m_host(host),
	m_user(user),
	m_password(password),
	m_path(path),
	m_port(port),
	m_scheme(scheme)
{
	
}

std::string amqp::Address::toString() const
{
	return (m_scheme + "://" + m_user + ":" + m_password
		+ "@" + m_host + ":" + std::to_string(m_port) + "/" + m_path);
}

void amqp::Address::setHost(std::string const &host)
{
	m_host = host;
}

void amqp::Address::setUser(std::string const &user)
{
	m_user = user;
}

void amqp::Address::setPassword(std::string const &password)
{
	m_password = password;
}

void amqp::Address::setPath(std::string const &path)
{
	m_path = path;
}

void amqp::Address::setPort(int port)
{
	m_port = port;
}

void amqp::Address::setScheme(std::string const &scheme)
{
	m_scheme = scheme;
}

void amqp::Address::setAddress(std::string const &host,
	std::string const &user,
	std::string const &password,
	std::string const &path,
	int port,
	std::string const &scheme)
{
	setHost(host);
	setUser(user);
	setPassword(password);
	setPath(path);
	setPort(port);
	setScheme(scheme);
}


std::string const &amqp::Address::getHost() const
{
	return (m_host);
}

std::string const &amqp::Address::getUser() const
{
	return (m_user);
}

std::string const &amqp::Address::getPassword() const
{
	return (m_password);
}

std::string const &amqp::Address::getPath() const
{
	return (m_path);
}

int amqp::Address::getPort() const
{
	return (m_port);
}

std::string const &amqp::Address::getScheme() const
{
	return (m_scheme);
}