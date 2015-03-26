/* 
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
 
 -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= 
 
 Based upon code from Sunfounder Microsophone Sensor Module  http://www.sunfounder.com/index.php?c=case_in&a=detail_&id=139&name= using a different but similar analog sound sensor purchased online.
 
 -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= 
 Arduino code to read data from a simple sound sensor, then augment and format as JSON to send via serial connection.
 Example of sending sound level data to Microsoft Azure and analyzing with Azure Stream Analytics or Azure Machine Learning.
*/
// Constants used for the ConnectTheDots project
//-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
// Constants used to add self-describing fields to the data before sending to Azure
// Disp value will be the label for the curve on the chart
// GUID ensures all the data from this sensor appears on the same chart
// You can use Visual Studio to create DeviceGUID and copy it here. In VS, On the Tools menu, click Create GUID. The Create GUID
// tool appears with a GUID in the Result box. Click Copy, and paste below.
//
char GUID[] = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
char Org[] = "My organization";
char Disp[] = "Arduino + Sound sensor";
char Locn[] = "here";
char Measure[] = "Sound level";
char Units[] = "units";
char buffer[300];
char dtostrfbuffer[15];

const int SOUND_PIN = A2;
const int SAMPLE_FREQUENCY = 250;
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
   
      // print string for temperature, separated by line for ease of reading
  // sent as one Serial.Print to reduce risk of serial errors
  
  memset(buffer,'\0',sizeof(buffer));
  strcat(buffer,"{");
  strcat(buffer,"\"guid\":\"");
  strcat(buffer,GUID);
  strcat(buffer,"\",\"organization\":\"");
  strcat(buffer,Org);
  strcat(buffer,"\",\"displayname\":\"");
  strcat(buffer,Disp);
  strcat(buffer,"\",\"location\":\"");
  strcat(buffer,Locn);  
  strcat(buffer,"\",\"measurename\":\"");
  strcat(buffer,Measure);
  strcat(buffer,"\",\"unitofmeasure\":\"");
  strcat(buffer,Units);
  strcat(buffer,"\",\"value\":");
  strcat(buffer,dtostrf(high,8,2,dtostrfbuffer));
  strcat(buffer,"}");
  Serial.println(buffer);
  delay(100); //just here to slow down the output so it is easier to read
    
  high = 0;
  hightime = millis();
  
  }
}
