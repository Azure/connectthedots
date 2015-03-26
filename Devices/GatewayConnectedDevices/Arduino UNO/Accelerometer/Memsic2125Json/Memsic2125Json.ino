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

Based upon David A. Mellis's Memsic2125 Example, as stated below, which is in the public domain

Modifications by Neal Analytics and Microsoft Open Technologies, Inc include adding self-describing fields in each output string, and changing output format to JSON string. the labels on charts in the Azure website.
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

// Disp value will be the label for the curve on the chart
// GUID ensures all the data from this sensor appears on the same chart
// You can use Visual Studio to create DeviceGUID and copy it here. In VS, On the Tools menu, click Create GUID. The Create GUID
// tool appears with a GUID in the Result box. Click Copy, and paste below.
//
char GUID1[] = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
char GUID2[] = "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy";
char Org[] = "My organization";
char Disp[] = "Arduino + Memsic2125";
char Locn[] = "here";
char Measure1[] = "accelerationX";
char Units1[] = "ft/sec/sec";
char Measure2[] = "accelerationY";
char Units2[] = "ft/sec/sec";
char buffer[300];
char dtostrfbuffer[15];

#define MYSERIAL Serial

const int xPin = 2; // X output of the accelerometer
const int yPin = 3; // Y output of the accelerometer
void setup() {
  // initialize serial communications:
  Serial.begin(9600);
  // initialize the pins connected to the accelerometer
  // as inputs:
  pinMode(xPin, INPUT);
  pinMode(yPin, INPUT);
}

void loop() {
  // variables to read the pulse widths:
  int pulseX, pulseY;
  // variables to contain the resulting accelerations
  int accelerationX, accelerationY;
  // read pulse from x‐ and y‐axes:
  pulseX = pulseIn(xPin,HIGH);
  pulseY = pulseIn(yPin,HIGH);
  // convert the pulse width into acceleration
  // accelerationX and accelerationY are in milli‐g's:
  // earth's gravity is 1000 milli‐g's, or 1g.
  accelerationX = ((pulseX / 10) - 500) * 8;
  accelerationY = ((pulseY / 10) - 500) * 8;
  
  // print string for humidity, separated by line for ease of reading
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
  strcat(buffer,dtostrf(accelerationX,8,2,dtostrfbuffer));
  strcat(buffer,"}");
  MYSERIAL.println(buffer);
  
  // print string for humidity, separated by line for ease of reading
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
  strcat(buffer,dtostrf(accelerationY,6,2,dtostrfbuffer));
  strcat(buffer,"}");
  MYSERIAL.println(buffer);
  
  delay(100);

}
