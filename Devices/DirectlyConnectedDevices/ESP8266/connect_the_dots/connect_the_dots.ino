// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Please use an Arduino IDE 1.6.8 or greater

#if ARDUINO_ARCH_ESP8266 //============================
// for ESP8266
#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h>
#include <WiFiUdp.h>

static WiFiClientSecure sslClient;

#elif ARDUINO_SAMD_FEATHER_M0 //-----------------------
// for Adafruit WINC1500
#include <Adafruit_WINC1500.h>
#include <Adafruit_WINC1500SSLClient.h>
#include <Adafruit_WINC1500Udp.h>
#include <NTPClient.h>

// for the Adafruit WINC1500 we need to create our own WiFi instance
// Define the WINC1500 board connections below.
#define WINC_CS   8
#define WINC_IRQ  7
#define WINC_RST  4
#define WINC_EN   2     // or, tie EN to VCC
// Setup the WINC1500 connection with the pins above and the default hardware SPI.
Adafruit_WINC1500 WiFi(WINC_CS, WINC_IRQ, WINC_RST);
static Adafruit_WINC1500SSLClient sslClient; // for Adafruit WINC1500

#else //-----------------------------------------------
// For WiFi101 based boards/shields
#include <WiFi101.h>
#include <WiFiSSLClient.h>
#include <WiFiUdp.h>
static WiFiSSLClient sslClient;
#endif //==============================================


#include <AzureIoTHub.h>

#include "connect_the_dots.h"

static char ssid[] = "[Your WiFi network SSID or name]";
static char pass[] = "[Your WiFi network WPA password or WEP key]";

/*
 * The new version of AzureIoTHub library change the AzureIoTHubClient signature.
 * As a temporary solution, we will test the definition of AzureIoTHubVersion, which is only defined 
 *    in the new AzureIoTHub library version. Once we totally deprecate the last version, we can take 
 *    the ‘#ifdef’ out.
 */
#ifdef AzureIoTHubVersion
static AzureIoTHubClient iotHubClient;
#else
AzureIoTHubClient iotHubClient(sslClient);
#endif

void setup() {
    initSerial();
    initWifi();
    initTime();

#ifdef AzureIoTHubVersion
    iotHubClient.begin(sslClient);
#else
    iotHubClient.begin();
#endif
}

void loop() {
    // Run the Connect The Dots from the Azure IoT Hub C SDK
    // You must set the device id, device key, IoT Hub name and IotHub suffix in
    // connect_the_dots.c
    connect_the_dots_run();
}

void initSerial() {

#if ARDUINO_ARCH_ESP8266 //============================
  // For ESP8266 boards
    Serial.begin(115200);
    Serial.setDebugOutput(true);
#else //-----------------------------------------------
  // For SAMD boards (e.g. MKR1000, Adafruit WINC1500 based)
  Serial.begin(9600);
#endif //==============================================
}

void initWifi() {

#if ARDUINO_SAMD_FEATHER_M0 //=========================
  // for the Adafruit WINC1500 we need to enable the chip
  pinMode(WINC_EN, OUTPUT);
  digitalWrite(WINC_EN, HIGH);
#endif //==============================================
 
    // Attempt to connect to Wifi network:
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(ssid);

    // Connect to WPA/WPA2 network. Change this line if using open or WEP network:
    WiFi.begin(ssid, pass);
    while (WiFi.status() != WL_CONNECTED) {
      delay(500);
      Serial.print(".");
    }
    Serial.println("Connected to wifi");
}

void initTime() {
#if ARDUINO_ARCH_ESP8266 //============================
  time_t epochTime;

  configTime(0, 0, "pool.ntp.org", "time.nist.gov");

  while (true) {
    epochTime = time(NULL);
    if (epochTime == 0) {
      Serial.println("Fetching NTP epoch time failed! Waiting 2 seconds to retry.");
      delay(2000);
    } else {
      Serial.print("Fetched NTP epoch time is: ");
      Serial.println(epochTime);
      break;
    }
  }

#elif ARDUINO_SAMD_FEATHER_M0 //-----------------------
  Adafruit_WINC1500UDP ntpUdp;
  NTPClient ntpClient(ntpUdp);

  ntpClient.begin();

  while (!ntpClient.update()) {
    Serial.println("Fetching NTP epoch time failed! Waiting 5 seconds to retry.");
    delay(5000);
  }

  ntpClient.end();

  unsigned long epochTime = ntpClient.getEpochTime();

  Serial.print("Fetched NTP epoch time is: ");
  Serial.println(epochTime);

  iotHubClient.setEpochTime(epochTime);

#else -------------------------------------------------
  // for WiFi101 based boards
  WiFiUDP ntpUdp;
  NTPClient ntpClient(ntpUdp);

  ntpClient.begin();

  while (!ntpClient.update()) {
    Serial.println("Fetching NTP epoch time failed! Waiting 5 seconds to retry.");
    delay(5000);
  }

  ntpClient.end();

  unsigned long epochTime = ntpClient.getEpochTime();

  Serial.print("Fetched NTP epoch time is: ");
  Serial.println(epochTime);

  iotHubClient.setEpochTime(epochTime);
#endif ===============================================
}
