// This #include statement was automatically added by the Spark IDE.
#include "Adafruit_DHT/Adafruit_DHT.h"

#define DHTPIN 2    // what pin we're connected to

// Uncomment whatever type you're using!
#define DHTTYPE DHT11		// DHT 11 
//#define DHTTYPE DHT22		// DHT 22 (AM2302)
//#define DHTTYPE DHT21		// DHT 21 (AM2301)

// Connect pin 1 (on the left) of the sensor to +5V
// Connect pin 2 of the sensor to whatever your DHTPIN is
// Connect pin 4 (on the right) of the sensor to GROUND
// Connect a 10K resistor from pin 2 (data) to pin 1 (power) of the sensor

DHT dht(DHTPIN, DHTTYPE);

// This #include statement was automatically added by the Spark IDE.
#include "HttpClient/HttpClient.h"

// This #include statement was automatically added by the Spark IDE.
#include "SparkTime/SparkTime.h"
  

char Org[] = "ORGANIZATIONNAME";
char Disp[] = "DISPLAYNAME";
char Locn[] = "LOCATION";

String AzureMobileService = "MOBILESERVICE.azure-mobile.net.azure-mobile.net";
String AzureMobileSeriveAPI = "APINAME";
char AzureMobileServiceKey[40] = "MOBILESERVICEKEY";

UDP UDPClient;
SparkTime rtc;
HttpClient http;
  

void setup()
{
    rtc.begin(&UDPClient, "north-america.pool.ntp.org");
    rtc.setTimeZone(-5); // gmt offset
    Serial.begin(9600);
    dht.begin();
    delay(10000);
}

 
void loop()
{
    delay(5000);
    
    unsigned long currentTime;
    currentTime = rtc.now();
    
    String timeNowString = rtc.ISODateUTCString(currentTime);
    char timeNowChar[sizeof(timeNowString)]; 
    strcpy(timeNowChar, timeNowString.c_str());
    
// Reading temperature or humidity takes about 250 milliseconds!
// Sensor readings may also be up to 2 seconds 'old' (its a 
// very slow sensor)
	float h = dht.getHumidity();
// Read temperature as Celsius
    float t = dht.getTempCelcius();
// Read temperature as Farenheit
	float f = dht.getTempFarenheit();
  
// Check if any reads failed and exit early (to try again).
//	if (isnan(h) || isnan(t) || isnan(f)) {
//		Serial.println("Failed to read from DHT sensor!");
//		return;
//	}   
   
    char payload[300];
    snprintf(payload, sizeof(payload), "{ \"s\":\"wthr\", \"u\":\"F\",\"l\":\"%s\",\"m\":\"Temperature\",\"t\":\"%s\",\"o\":\"%s\",\"g\":\"00000000-0000-0000-0000-000000000000\",\"v\": %f,\"d\":\"%s\" }", Locn, timeNowString.c_str(), Org, f, Disp);
    Spark.publish("ConnectTheDots", payload);
    
    snprintf(payload, sizeof(payload), "{ \"s\":\"wthr\", \"u\":\"%%\",\"l\":\"%s\",\"m\":\"Humidity\",\"t\":\"%s\",\"o\":\"%s\",\"g\":\"00000000-0000-0000-0000-000000000000\",\"v\": %f,\"d\":\"%s\" }", Locn, timeNowString.c_str(), Org, h, Disp);
    Spark.publish("ConnectTheDots", payload);
}