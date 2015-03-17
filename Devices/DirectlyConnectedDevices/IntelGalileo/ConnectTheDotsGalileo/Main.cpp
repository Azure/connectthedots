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

#include "stdafx.h"
#include "arduino.h"

// Sparkfun Weather Shield libraries
#include "HTU21D.h"
#include "MPL3115A2.h"
// Proton library is used to send AMQP messages to Azure Event Hubs
#include "ProtonSender.h"
// Json library
#include "json.h"
// XML parsing library for reading configuration file
#include "rapidxml.hpp"
#include "rapidxml_utils.hpp"

// The code below can be used to send simulated data if you don't have a weather shield
// #define SIMULATEDATA
// #define USEONBOARDSENSOR

// Handles to pressure and humidity sensors from the Weather shield
MPL3115A2 myPressure;
HTU21D myHumidity;

#ifdef USEONBOARDSENSOR
int tempPin = -1; // The on-board thermal sensor for Galileo V1 boards
#endif

// structure used to store application settings read from XML file
typedef struct tAppSettings
{
	char sbnamespace[128];  //i.e. ConnectTheDots-ns
	char entity[128];       //EventHub Name
	char issuerName[128];   //Key Issuer
	char issuerKey[128];    //i.e. "44CHARACTERKEYFOLLOWEDBYEQUALSSIGN=" //****URL DECODED*****
	char* sbDomain = "servicebus.windows.net";
	char* subject = "wthr";
	char name[128];
	char guid[128];
	char location[128];
	char organization[128];
};
tAppSettings appSettings;

typedef struct tSensorSettings
{
	char measurename[128];
	char unitofmeasure[128];
	double value;
};

tSensorSettings* sensorSettings;
int sensorCount = 0;


// Utility function to retreive and store specific app settings
void StoreAppConfigurationAttribute(char* key, char* value)
{
	if (!strcmp(key, "namespace")) strncpy(appSettings.sbnamespace, value, strlen(value)+1);
	else if (!strcmp(key, "keyname")) strncpy(appSettings.issuerName, value, strlen(value) + 1);
	else if (!strcmp(key, "key")) strncpy(appSettings.issuerKey, value, strlen(value) + 1);
	else if (!strcmp(key, "eventhubname")) strncpy(appSettings.entity, value, strlen(value) + 1);
	else if (!strcmp(key, "name")) strncpy(appSettings.name, value, strlen(value) + 1);
	else if (!strcmp(key, "guid")) strncpy(appSettings.guid, value, strlen(value) + 1);
	else if (!strcmp(key, "location")) strncpy(appSettings.location, value, strlen(value) + 1);
	else if (!strcmp(key, "organization")) strncpy(appSettings.organization, value, strlen(value) + 1);
}

void StoreSensorConfigurationAttribute(char* key, char* value, int sensorIndex)
{
	if (!strcmp(key, "measurename")) strncpy(sensorSettings[sensorIndex].measurename, value, strlen(value) + 1);
	else if (!strcmp(key, "unitofmeasure")) strncpy(sensorSettings[sensorIndex].unitofmeasure, value, strlen(value) + 1);
}


// Read configuration file which is an XML file
void ReadConfiguration(char* filePath)
{
	using namespace rapidxml;

	// Get configuration file into a dom object
	file<> xmlFile(filePath);
	xml_document<> doc;
	doc.parse<0>(xmlFile.data());
	
	// parse for app settings
	xml_node<> *node = doc.first_node("configuration")->first_node("appSettings")->first_node("add");
	while (node)
	{
		StoreAppConfigurationAttribute(node->first_attribute("key")->value(), node->first_attribute("value")->value());
		node = node->next_sibling("add");
	}

	// Count number of sensors listed in the settings file
	node = doc.first_node("configuration")->first_node("sensorSettings");
	while (node)
	{
		sensorCount++;
		node = node->next_sibling("sensorSettings");
	}

	if (sensorCount > 0)
	{
		// Allocate table for sensors settings
		sensorSettings = new tSensorSettings[sensorCount];
		ZeroMemory(sensorSettings, sensorCount * sizeof(tSensorSettings));

		// parse for sensors settings
		int sensorIndex = 0;
		node = doc.first_node("configuration")->first_node("sensorSettings");
		while (node)
		{
			xml_node<> *sensorNode = node->first_node("add");
			while (sensorNode)
			{
				StoreSensorConfigurationAttribute(sensorNode->first_attribute("key")->value(), sensorNode->first_attribute("value")->value(), sensorIndex);
				sensorNode = sensorNode->next_sibling("add");
			}

			node = node->next_sibling("sensorSettings");
			sensorIndex++;
		}
	}

}

int _tmain(int argc, _TCHAR* argv[])
{
	return RunArduinoSketch();
}

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

// During Setup we read the configuration file to retreive connections settings and we initialize the Sensors
void setup()
{
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

	ReadConfiguration("C:\\ConnectTheDots\\ConnectTheDotsGalileo.exe.config");

	InitWeatherShieldSensors();
}

// Struture used to 
typedef struct SensorData
{
	float tempC;
	float tempF;
	float hmdt;
};

//SensorData ReadSensorData()
void ReadSensorData()
{
	SensorData data;

#ifdef SIMULATEDATA
	data.tempC = 75;	// Storage for the temperature value
	data.tempF = rand() % 100 + 60;
	data.hmdt = rand() % 100;
#else
#ifdef USEONBOARDSENSOR
	// reads the analog value from this pin (values range from 0-1023)
	data.tempC = (float) analogRead(tempPin);
	data.tempF = data.tempC * 9 / 5 + 32;
	data.hmdt = 25;
#endif
	data.tempC = myHumidity.readTemperature();
	data.tempF = (data.tempC *9.0) / 5.0 + 32.0;
	data.hmdt = myHumidity.readHumidity();
#endif

	for (int i = 0; i < sensorCount; i++)
	{
		if (!strcmp(sensorSettings[i].measurename, "Temperature")) sensorSettings[i].value = data.tempF;
		else if (!strcmp(sensorSettings[i].measurename, "Humidity")) sensorSettings[i].value = data.hmdt;
	}
}

void GetTimeNow(pn_timestamp_t* pUtcTime, char* pTimeNow)
{
	// We need to time stamp the AMQP message.
	// Getting UTC time
	struct tm timeinfo;

	time(pUtcTime);
	gmtime_s(&timeinfo, pUtcTime);
	strftime(pTimeNow, 80, "%Y-%m-%dT%H:%M:%SZ", &timeinfo);
	puts(pTimeNow);
}

WCHAR* char2WCHAR(char* text)
{
	WCHAR* output = (WCHAR*) calloc(strlen(text) + 1, sizeof(WCHAR));
	mbstowcs(output, text, strlen(text));
	return output;
}

void SendAMQPMessage(char* message, pn_timestamp_t utcTime)
{
	sender(appSettings.sbnamespace,
			appSettings.entity,
			appSettings.issuerName,
			appSettings.issuerKey,
			appSettings.sbDomain,
		appSettings.name,
			appSettings.subject,
			message,
			utcTime);
}

void SendSensorData(tSensorSettings sensorSettings, pn_timestamp_t utcTime, char* timeNow)
{
	// Create JSON packet
	JSONObject jsonData;

	jsonData[L"unitofmeasure"] = new JSONValue(char2WCHAR(sensorSettings.unitofmeasure));
	jsonData[L"measurename"] = new JSONValue(char2WCHAR(sensorSettings.measurename));
	jsonData[L"location"] = new JSONValue(char2WCHAR(appSettings.location));
	jsonData[L"organization"] = new JSONValue(char2WCHAR(appSettings.organization));
	jsonData[L"guid"] = new JSONValue(char2WCHAR(appSettings.guid));
	jsonData[L"displayname"] = new JSONValue(char2WCHAR(appSettings.name));
	jsonData[L"value"] = new JSONValue((double) sensorSettings.value);
	jsonData[L"subject"] = new JSONValue(char2WCHAR(appSettings.subject));
	jsonData[L"timecreated"] = new JSONValue(char2WCHAR(timeNow));

	JSONValue *value = new JSONValue(jsonData);
	std::wstring serializedData = value->Stringify();
	char* msgText = (char*) calloc(serializedData.length() + 1, sizeof(char));
	wcstombs(msgText, serializedData.c_str(), serializedData.length() * sizeof(char));

	// Send AMQP message
	SendAMQPMessage(msgText, utcTime);
}



// the loop routine runs over and over again forever:
void loop()
{
	// Get current time
	pn_timestamp_t utcTime;
	char timeNow[80];
	GetTimeNow(&utcTime, timeNow);

	// Read data from sensors
	ReadSensorData();

	// Send data for each sensor
	for (int sensorIndex = 0; sensorIndex < sensorCount; sensorIndex++)
	{
		SendSensorData(sensorSettings[sensorIndex], utcTime, timeNow);
		Sleep(500);
	}
}