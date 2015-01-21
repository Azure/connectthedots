#pragma once

#ifndef _AMQP_ADDRESS
#define _AMQP_ADDRESS

#include <string>

namespace amqp {

	class Address 
	{
	public:

		Address(std::string const &host = "", 
			std::string const &user = "",
			std::string const &password = "",
			std::string const &path = "",
			int port = 5671,
			std::string const &scheme = "amqps");

		virtual ~Address()
		{
		};

		std::string toString() const;

		void setAddress(std::string const &host,
			std::string const &user,
			std::string const &password,
			std::string const &path,
			int port = 5671,
			std::string const &scheme = "amqps");

		void setHost(std::string const &host);
		void setUser(std::string const &user);
		void setPassword(std::string const &password);
		void setPath(std::string const &path);
		void setPort(int port);
		void setScheme(std::string const &scheme);

		std::string const &getHost() const;
		std::string const &getUser() const;
		std::string const &getPassword() const;
		std::string const &getPath() const;
		int getPort() const;
		std::string const &getScheme() const;

	private:
		std::string m_host;
		std::string m_user;
		std::string m_password;
		std::string m_path;
		std::string m_scheme;
		int m_port;
	};


};

#endif // _AMQP_ADDRESS