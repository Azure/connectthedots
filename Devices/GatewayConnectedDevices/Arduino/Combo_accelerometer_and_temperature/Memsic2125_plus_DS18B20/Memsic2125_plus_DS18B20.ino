/*
Copyright (c) Neal Analytics, Inc.  All rights reserved.

The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

 -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

Based upon David A. Mellis's Memsic2125 Example, as stated below, which is in the public domain.

Modifications by Neal Analytics and Microsoft Open Technologies, Inc include adding self-describing fields in each output string, and changing output format to JSON string. 
Further mods by MS Open Tech updating JSON for website rev

-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

Original header follows.

Memsic2125
Read the Memsic 2125 two‐axis accelerometer. Converts the
pulses output by the 2125 into milli‐g's (1/1000 of earth's
gravity) and prints them over the serial connection to the
computer.
The circuit:
* X output of accelerometer to digital pin 2
* Y output of accelerometer to digital pin 3
* +V of accelerometer to +5V
* GND of accelerometer to ground
http://www.arduino.cc/en/Tutorial/Memsic2125
created 6 Nov 2008
by David A. Mellis
modified 30 Aug 2011
by Tom Igoe
This example code is in the public domain.
*/
// these constants won't change:



#include <OneWire.h> 

char SensorSubject[] = "wthr";                                // determines how Azure website will chart the data
char DeviceDisplayName[] = "Cooler Device";                   // will be the label for the curve on the chart
char DeviceGUID[] = "2150719D-0FFF-4312-B61C-75AD5219D8FF";   // ensures all the data from this sensor appears on the same chart. You can use the Tools/Create GUID in Visual Studio to create

// Disp value will be the label for the curve on the chart
// GUID ensures all the data from this sensor appears on the same chart
// You can use Visual Studio to create DeviceGUID and copy it here. In VS, On the Tools menu, click Create GUID. The Create GUID
// tool appears with a GUID in the Result box. Click Copy, and paste below.
//
char GUID1[] = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
char GUID2[] = "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy";
char Org[] = "My organization";
char Disp[] = "Arduino + Memsic2125 + DS18S20";
char Locn[] = "here";
char Measure1[] = "acceleration_Tot";
char Units1[] = "ft/sec/sec";
char Measure2[] = "temperature";
char Units2[] = "F";
char buffer[300];
char dtostrfbuffer[15];

#define MYSERIAL Serial

const int DS18S20_Pin = 4; //DS18S20 Signal pin on digital 4
const int xPin = 2; // X output of the accelerometer
const int yPin = 3; // Y output of the accelerometer

//Temperature chip i/o
OneWire ds(DS18S20_Pin);  // on digital pin 4

void setup() {
  // initialize serial communications:
  Serial.begin(9600);
  // initialize the pins connected to the accelerometer
  // as inputs:
  pinMode(xPin, INPUT);
  pinMode(yPin, INPUT);
  //pinMode(DS18S20_Pin, INPUT);
}


float getTemp(){
  //returns the temperature from one DS18S20 in DEG Celsius

  byte data[12];
  byte addr[8];

  if ( !ds.search(addr)) {
      //no more sensors on chain, reset search
      ds.reset_search();
      return -1000;
  }

  if ( OneWire::crc8( addr, 7) != addr[7]) {
      Serial.println("CRC is not valid!");
      return -1000;
  }

  if ( addr[0] != 0x10 && addr[0] != 0x28) {
      Serial.print("Device is not recognized");
      return -1000;
  }

  ds.reset();
  ds.select(addr);
  ds.write(0x44,1); // start conversion, with parasite power on at the end

  byte present = ds.reset();
  ds.select(addr);    
  ds.write(0xBE); // Read Scratchpad

  
  for (int i = 0; i < 9; i++) { // we need 9 bytes
    data[i] = ds.read();
  }
  
  ds.reset_search();
  
  byte MSB = data[1];
  byte LSB = data[0];

  float tempRead = ((MSB << 8) | LSB); //using two's compliment
  float TemperatureSum = tempRead / 16;
  
  return TemperatureSum;
  
}


void loop() {
  // variables to read the pulse widths:
  int pulseX, pulseY;
  // variables to contain the resulting accelerations
  float accelerationX, accelerationY, acceleration_Tot;
  // read pulse from x‐ and y‐axes:
  pulseX = pulseIn(xPin,HIGH);
  pulseY = pulseIn(yPin,HIGH);
  // convert the pulse width into acceleration
  // accelerationX and accelerationY are in milli‐g's:
  // earth's gravity is 1000 milli‐g's, or 1g.
  accelerationX = ((pulseX / 10) - 500) * 8;
  accelerationY = ((pulseY / 10) - 500) * 8;
  acceleration_Tot = sqrt(sq(accelerationX) + sq(accelerationY));
  
  //acceleration_Tot = sqrt(sq(pulseX) + sq(pulseY));
  
  
  // Get temperature reading.  Adjust value to match cooler
  float temperature = getTemp();
  
  // print string for total acceleration, separated by line for ease of reading
  memset(buffer,'\0',sizeof(buffer));
  strcat(buffer,"{");
  strcat(buffer,"\"guid\":\"");
  strcat(buffer,GUID1);
  strcat(buffer,"\",\"organization\":\"");
  strcat(buffer,Org);
  strcat(buffer,"\",\"displayname\":\"");
  strcat(buffer,Disp);
  strcat(buffer,"\",\"location\":\"");
  strcat(buffer,Locn);  
  strcat(buffer,"\",\"measurename\":\"");
  strcat(buffer,Measure1);
  strcat(buffer,"\",\"unitofmeasure\":\"");
  strcat(buffer,Units1);
  strcat(buffer,"\",\"value\":");
  strcat(buffer,dtostrf(acceleration_Tot,8,2,dtostrfbuffer));
  strcat(buffer,"}");
  MYSERIAL.println(buffer);
  
  // print string for temperature, separated by line for ease of reading
  memset(buffer,'\0',sizeof(buffer));
  strcat(buffer,"{");
  strcat(buffer,"\"guid\":\"");
  strcat(buffer,GUID2);
  strcat(buffer,"\",\"organization\":\"");
  strcat(buffer,Org);
  strcat(buffer,"\",\"displayname\":\"");
  strcat(buffer,Disp);
  strcat(buffer,"\",\"location\":\"");
  strcat(buffer,Locn);  
  strcat(buffer,"\",\"measurename\":\"");
  strcat(buffer,Measure2);
  strcat(buffer,"\",\"unitofmeasure\":\"");
  strcat(buffer,Units2);
  strcat(buffer,"\",\"value\":");
  strcat(buffer,dtostrf(temperature,6,2,dtostrfbuffer));
  strcat(buffer,"}");
  MYSERIAL.println(buffer);


}
