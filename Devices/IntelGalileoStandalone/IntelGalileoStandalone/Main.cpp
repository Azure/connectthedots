#include "stdafx.h"
#include "arduino.h"

#include "ProtonSender.h"

#define SIMULATEDATA true;

int _tmain(int argc, _TCHAR* argv[])
{
	return RunArduinoSketch();
}

void setup()
{
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
}

char* deviceDisplayName = "Galileo";
char* sbnamespace = "NAMESPACE";  //i.e. ConnectTheDotsDX-ns
char* entity = "ehdevices"; //EventHub Name
char* issuerName = "D1"; //Key Issuer
//Key Must  be url-decoded - decode here: http://meyerweb.com/eric/tools/dencoder/
char* issuerKey = "ISSUERKEY"; //i.e. "44CHARACTERKEYFOLLOWEDBYEQUALSSIGN=" //****URL DECODED*****
char* sbDomain = "servicebus.windows.net";

int tempPin = -1; // The on-board thermal sensor for Galileo V1 boards

// the loop routine runs over and over again forever:
void loop()
{


#ifdef SIMULATEDATA
	float temperatureInDegreesC = 75;	// Storage for the temperature value
	float temperatureInF = rand() % 100 + 60;
#endif
#ifndef SIMULATEDATA
	//// reads the analog value from this pin (values range from 0-1023)
	float temperatureInDegreesC = (float)analogRead(tempPin);
	float temperatureInF = temperatureInDegreesC * 9 / 5 + 32;
#endif

	printf("Temperature: %lf Fahrenheit\n", temperatureInF);

	pn_timestamp_t utcTime;
	struct tm timeinfo;
	char timeNow[80];

	time(&utcTime);
	gmtime_s(&timeinfo, &utcTime);
	strftime(timeNow, 80, "%Y-%m-%dT%H:%M:%SZ", &timeinfo);
	puts(timeNow);

	char * subject = "wthr";
	char msgtext[500];

	_snprintf_s(msgtext, sizeof(msgtext),
		" {\"temp\":%.2f,\"Subject\":\"wthr\",\"time\":\"%s\",\"from\":\"%s\",\"dspl\":\"%s\"}",
		temperatureInF, timeNow, deviceDisplayName, deviceDisplayName);

	sender(sbnamespace, entity, issuerName, issuerKey, sbDomain, deviceDisplayName, subject, msgtext, utcTime);

}