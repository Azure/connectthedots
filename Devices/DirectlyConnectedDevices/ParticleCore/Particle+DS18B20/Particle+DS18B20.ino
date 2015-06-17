// This #include statement was automatically added by the Spark IDE.
#include "SparkButton/SparkButton.h"
#include "math.h"

// This #include statement was automatically added by the Spark IDE.
#include "OneWire.h"

// This #include statement was automatically added by the Spark IDE.
#include "DS18B20.h"

// This #include statement was automatically added by the Spark IDE.
#include "SparkTime/SparkTime.h"

// This #include statement was automatically added by the Spark IDE.
#include "HttpClient/HttpClient.h"

SparkButton b = SparkButton();

DS18B20 ds18b20 = DS18B20(D7);

char Org[] = "ORGANIZATIONNAME";
char Disp[] = "DISPLAYNAME";
char Locn[] = "LOCATION";

UDP UDPClient;
SparkTime rtc;
HttpClient http;
  

void setup()
{
    b.begin();
    rtc.begin(&UDPClient, "north-america.pool.ntp.org");
    rtc.setTimeZone(-5); // gmt offset
    Serial.begin(9600);
    
    delay(10000);
}

 
void loop()
{
    delay(5000);
    
    if(!ds18b20.search()){
        Serial.println("No more addresses.");
        Serial.println();
        ds18b20.resetsearch();
        delay(2000);
        
        return;       
    }
    
    float c = ds18b20.getTemperature();
    float f = ds18b20.convertToFahrenheit(c);
    
    if(f < 75)
        b.allLedsOn(0,255,255); //light blue
    else if(f > 75 && f < 80)
        b.allLedsOn(0,0,255); //blue
    else if(f > 85 && f < 90)
        b.allLedsOn(255,255,0); //yellow
    else if(f > 90 && f < 95)
        b.allLedsOn(255,165,0); //orange
    else if(f > 95 && f < 100)
        b.allLedsOn(255,69,0);  //red
    else if(f > 100)
        b.allLedsOn(255,0,0);   //very red
    
    unsigned long currentTime;
    currentTime = rtc.now();
    
    String timeNowString = rtc.ISODateUTCString(currentTime);
    char timeNowChar[sizeof(timeNowString)]; 
    strcpy(timeNowChar, timeNowString.c_str());
   
    char payload[300];
    
    snprintf(payload, sizeof(payload), "{ \"s\":\"wthr\", \"u\":\"F\",\"l\":\"%s\",\"m\":\"Temperature\",\"t\":\"%s\",\"o\":\"%s\",\"g\":\"00000000-0000-0000-0000-000000000000\",\"v\": %f,\"d\":\"%s\" }", Locn, timeNowString.c_str(), Org, f, Disp);
    Spark.publish("ConnectTheDots", payload);
    
    //snprintf(payload, sizeof(payload), "{ \"s\":\"wthr\", \"u\":\"%%\",\"l\":\"%s\",\"m\":\"Humidity\",\"t\":\"%s\",\"o\":\"%s\",\"g\":\"00000000-0000-0000-0000-000000000000\",\"v\": %f,\"d\":\"%s\" }", Locn, timeNowString.c_str(), Org, h, Disp);
    //Spark.publish("ConnectTheDots", payload);    
}