//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

#include <ctime>
#include <sstream>
#include <iomanip>
#include <locale>
#include "stdafx.h"
#include "arduino.h"

// The code below can be used to send simulated data if you don't have a weather shield
// #define SIMULATEDATA
// #define USEONBOARDSENSOR
// #define USEGROVESTARTERKIT
 #define USESPARKFUNWEATHERSHIELD
#define TRACKMESSAGES true

// Proton library is used to send AMQP messages to Azure Event Hubs
#include "amqp\amqp.h"

// Sparkfun Weather Shield libraries
#ifdef USESPARKFUNWEATHERSHIELD
#	include "HTU21D.h"
#	include "MPL3115A2.h"
#endif // USESPARKFUNWEATHERSHIELD

// Seeed Grove Starer Kit Plus for Intel Galileo
#ifdef USEGROVESTARTERKIT
#	include "grove\grove.h"
#endif // USEGROVESTARTERKIT

// Json library
#include "json.h"
// XML parsing library for reading configuration file
#include "rapidxml.hpp"
#include "rapidxml_utils.hpp"

// Handles to pressure and humidity sensors from the Weather shield
#ifdef USESPARKFUNWEATHERSHIELD
MPL3115A2 myPressure;
HTU21D myHumidity;
#endif // USESPARKFUNWEATHERSHIELD

#ifdef USEONBOARDSENSOR
int tempPin = -1; // The on-board thermal sensor for Galileo V1 boards
#endif // USEONBOARDSENSOR

#ifdef USEGROVESTARTERKIT
grove::Temperature temperatureSensor(A0);
grove::Light lightSensor(A1);
#endif // USEGROVESTARTERKIT

#define CONFIG_FILE_PATH "ConnectTheDotsGalileo.exe.config"

amqp::Address eventHubAddress;
amqp::Sender amqpSender(TRACKMESSAGES);

// structure used to store application settings read from XML file
typedef struct tAppSettings
{
	std::string deviceDisplayName;
	std::string deviceID;

	std::string host; // Azure Service Bus Host, i.e. ConnectTheDots-ns.servicebus.windows.net
	std::string path; //EventHub Name
	std::string user; // Key Issuer, Shared Access Policy Name
	std::string password; // Shared Access Key, i.e. "44CHARACTERKEYFOLLOWEDBYEQUALSSIGN=" 
	std::string subject = "wthr";

	// UTF versions of setting for message content
	std::wstring deviceDisplayNameW;
	std::wstring deviceIDW;
	std::wstring subjectW = L"wthr";
};

tAppSettings appSettings;

// Utility function to retreive and store specific app settings
void StoreConfigurationAttribute(char* key, char* value)
{
	std::wstring_convert< std::codecvt<wchar_t, char, std::mbstate_t> > conv;
	
	if (!strcmp(key, "DeviceName")) {
		appSettings.deviceDisplayName.assign(value);
		appSettings.deviceDisplayNameW = conv.from_bytes(appSettings.deviceDisplayName);
	}
	else if (!strcmp(key, "DeviceID")) { 
		appSettings.deviceID.assign(value); 
		appSettings.deviceIDW = conv.from_bytes(appSettings.deviceID);
	}
	else if (!strcmp(key, "Host")) appSettings.host.assign(value);
	else if (!strcmp(key, "Path")) appSettings.path.assign(value);
	else if (!strcmp(key, "User")) appSettings.user.assign(value);
	else if (!strcmp(key, "Password")) appSettings.password.assign(value);

}

// Read configuration file which is an XML file
void ReadConfiguration(char* filePath)
{
	using namespace rapidxml;

	// Get configuration file into a dom object
	file<> xmlFile(filePath);
	xml_document<> doc;
	doc.parse<0>(xmlFile.data());
	
	// parse for settings
	xml_node<> *node = doc.first_node("configuration")->first_node("appSettings")->first_node("add");
	while (node)
	{
		StoreConfigurationAttribute(node->first_attribute("key")->value(), node->first_attribute("value")->value());
		node = node->next_sibling("add");
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	return RunArduinoSketch();
}

#ifdef USESPARKFUNWEATHERSHIELD
// Function to intialize the Weather Shield sensors
// Communication is established over I2C
void InitWeatherShieldSensors()
{
	// initialize the digital pin as an output.
	Wire.begin();        // Join i2c bus

	// Test Multiple slave addresses:
	Wire.beginTransmission(0x40);
	Wire.write(0xE7);  // Address of data to get
	Wire.endTransmission(false); // Send data to I2C dev with option for a repeated start. THIS IS NECESSARY and not supported before Arduino V1.0.1!
	if (Wire.requestFrom(0x40, 1) != 1)
	{
		Log(L"Error reading from humidity sensor\n");
	}

	byte status = Wire.read();
	Log("Humidity sensor status 0x%0x\n\n", status);

	myHumidity.begin();
	myPressure.begin();

	// Configure the sensor
	myPressure.setModeBarometer(); // Measure pressure in Pascals from 20 to 110 kPa
	myPressure.setOversampleRate(7); // Set Oversample to the recommended 128
	myPressure.enableEventFlags(); // Enable all three pressure and temp event flags 
}
#endif // USESPARKFUNWEATHERSHIELD

// During Setup we read the configuration file to retreive connections settings and we initialize the Sensors
void setup()
{
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

	try {
		ReadConfiguration(CONFIG_FILE_PATH);
		eventHubAddress.setAddress(appSettings.host,
			appSettings.user,
			appSettings.password,
			appSettings.path);

#ifdef USESPARKFUNWEATHERSHIELD
	InitWeatherShieldSensors();
#endif // USESPARKFUNWEATHERSHIELD

	} catch (std::exception &e) {
		Log(e.what());
	}
}

// Struture used to 
typedef struct SensorData
{
	float tempC;
	float tempF;
	float hmdt;
	float lght; // light value in Lux
};

SensorData ReadSensorData()
{
	SensorData data;

#ifdef SIMULATEDATA
	data.tempC = 75.0;	// Storage for the temperature value
	data.tempF = static_cast<float>(rand() % 100 + 60);
	data.hmdt = static_cast<float>(rand() % 100);
	data.lght = static_cast<float>(rand() % 100);
#else

#	ifdef USEONBOARDSENSOR
	// reads the analog value from this pin (values range from 0-1023)
	data.tempC = static_cast<float>(analogRead(tempPin));
	data.tempF = data.tempC * 9.0 / 5.0 + 32.0;
	data.hmdt = 25.0;
	data.lght = 55.0;
#	endif // USEONBOARDSENSOR

#	ifdef USESPARKFUNWEATHERSHIELD
	data.tempC = myHumidity.readTemperature();
	data.tempF = (data.tempC *9.0) / 5.0 + 32.0;
	data.hmdt = myHumidity.readHumidity();
	data.lght = 55.0;
#	endif // USESPARKFUNWEATHERSHIELD

#	ifdef USEGROVESTARTERKIT
	// reads the analog value from this pin (values range from 0-1023)
	data.tempC = temperatureSensor.inC();
	data.tempF = data.tempC * 9 / 5 + 32;
	data.hmdt = 25;
	data.lght = lightSensor.inLux();
#	endif // USEGROVESTARTERKIT

#endif // SIMULATEDATA

	return data;
}

std::wstring GetTimeNow()
{
	// We need to time stamp the AMQP message.
	// Getting UTC time
	std::time_t currentTime = std::time(nullptr);

	std::wstringstream ostr;
	ostr << std::put_time(std::gmtime(&currentTime), L"%Y-%m-%dT%H:%M:%SZ");
	return (ostr.str());
}

// the loop routine runs over and over again forever:
void loop()
{
	try {
		// Read data from sensors
		SensorData data = ReadSensorData();

		// Create JSON packet
		JSONObject jsonData;

		jsonData[L"temp"] = new JSONValue(static_cast<double>(data.tempF));
		jsonData[L"hmdt"] = new JSONValue(static_cast<double>(data.hmdt));
		jsonData[L"lght"] = new JSONValue(static_cast<double>(data.lght));
		jsonData[L"subject"] = new JSONValue(appSettings.subjectW);
		jsonData[L"time"] = new JSONValue(GetTimeNow());
		jsonData[L"from"] = new JSONValue(appSettings.deviceIDW);
		jsonData[L"dspl"] = new JSONValue(appSettings.deviceDisplayNameW);

		amqp::JsonMessage telemetryMessage(appSettings.subject, jsonData, amqp::IMessage::UTF8);
		telemetryMessage.addAnnotation(amqp::AMQPSymbol("x-opt-partition-key"), amqp::AMQPuuid(appSettings.deviceID));
		telemetryMessage.addProperty(amqp::AMQPString("Subject"), amqp::AMQPString(appSettings.deviceDisplayName));
		
		// Send AMQP message
		amqpSender.send(telemetryMessage, eventHubAddress);

		Sleep(1000);
	}
	catch (std::exception &e) {
		Log(e.what());
	}
}