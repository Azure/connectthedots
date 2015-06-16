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

String AzureMobileService = "MOBILESERVICE.azure-mobile.net.azure-mobile.net";
String AzureMobileSeriveAPI = "APINAME";
char AzureMobileServiceKey[40] = "MOBILESERVICEKEY";

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
    snprintf(payload, sizeof(payload), "{ \"subject\":\"wthr\", \"unitofmeasure\":\"F\",\"location\":\"%s\",\"measurename\":\"Temperature\",\"timecreated\":\"%s\",\"organization\":\"%s\",\"guid\":\"00000000-0000-0000-0000-000000000001\",\"value\": %f,\"displayname\":\"%s\" }", Locn, timeNowChar, Org, f, Disp);
    sendToAMS(payload);
    //strcpy(timeNowChar, timeNowString.c_str());
    //snprintf(payload, sizeof(payload), "{ \"subject\":\"wthr\", \"unitofmeasure\":\"%%\",\"location\":\"%s\",\"measurename\":\"Humidity\",\"timecreated\":\"%s\",\"organization\":\"%s\",\"guid\":\"00000000-0000-0000-0000-000000000001\",\"value\": %f,\"displayname\":\"%s\" }", Locn, timeNowChar, Org, h, Disp);
    //sendToAMS(payload);
    
}

void sendToAMS(String payload)
{
       http_header_t headers[] = {
        { "X-ZUMO-APPLICATION", AzureMobileServiceKey },
        { "Cache-Control", "no-cache" },
        { NULL, NULL } // NOTE: Always terminate headers with NULL
    };
    
    http_request_t request;
    http_response_t response;
    
    request.hostname = AzureMobileService;
    request.port = 80;
    request.path = "/api/" + AzureMobileSeriveAPI;
    request.body = payload;

    http.post(request, response, headers);
    Serial.print("Application>\tResponse status: ");
    Serial.println(response.status);

    Serial.print("Application>\tHTTP Response Body: ");
    Serial.println(response.body);
}