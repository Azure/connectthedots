/* 
 Simple sound sensor code for http://connectthedots.msopentech.com end-to-end example of sending data to Microsoft Azure
 By: Microsoft Open Technologies, Inc.
 Date: January 27, 2015
  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

 Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.

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
 
 Based upon code from Sunfounder Microsophone Sensor Module  http://www.sunfounder.com/index.php?c=case_in&a=detail_&id=139&name= using a different but similar analog sound sensor purchased online.
 
 Modifications by Microsoft Open Technologies, Inc include adding self-describing fields in each output string, and changing 
 output format to JSON string. Based upon the variables below, the JSON string would be something like the following, depending upon the values retrieved
 from the Weather Shield:

 {"dspl":"Hex Sound Sensor 01","Subject":"sound","DeviceGUID":"898A4B4F-80B7-4BA0-B4B1-186655336472","millis":80176,"seqno":79,"soundLvl":78}
 
 The dspl, Subject, deviceGUID data may be used by the Azure website and services to identify the device sending the data. The subsequent field names may be used to generate the labels on charts in the Azure website.
 -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

*/
// Constants used for the ConnectTheDots project
//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
// SensorSubject value determines how Azure website will chart the data
// DeviceDisplayName value will be the label for the curve on the chart
// DeviceGUID ensures all the data from this sensor appears on the same chart
// You can use Visual Studio to create DeviceGUID and copy it here. In VS, On the Tools menu, click Create GUID. The Create GUID
// tool appears with a GUID in the Result box. Click Copy, and paste below.
//
char SensorSubject[] = "sound";
char DeviceDisplayName[] = "Hex Sound Sensor 01";
char DeviceGUID[] = "898A4B4F-80B7-4BA0-B4B1-186655336472";
//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


const int SOUND_PIN = A0;
const int SAMPLE_FREQUENCY = 500;
int high = 0;
unsigned long hightime = millis();
int sequenceNumber = 0;

void setup() {
  //pinMode(SOUND_PIN,INPUT);
  Serial.begin(9600);
}

void loop() {
  int value = analogRead(SOUND_PIN);
  //int value = digitalRead(SOUND_PIN);
  if (high < value) {
    high = value;
  }
  if (millis() - hightime >= SAMPLE_FREQUENCY) {
    Serial.print("{");
    Serial.print("\"dspl\":");
    Serial.print("\"");
    Serial.print(DeviceDisplayName);
    Serial.print("\"");
    Serial.print(",\"Subject\":");
    Serial.print("\"");
    Serial.print(SensorSubject);
    Serial.print("\"");
    Serial.print(",\"DeviceGUID\":");
    Serial.print("\"");
    Serial.print(DeviceGUID);
    Serial.print("\"");
    Serial.print(",\"millis\":");
    Serial.print(millis());
    Serial.print(",\"seqno\":");
    Serial.print(sequenceNumber++);
    Serial.print(",\"soundLvl\":");
    Serial.print(high);
    Serial.println("}");
    
    high = 0;
    hightime = millis();
  }
}
