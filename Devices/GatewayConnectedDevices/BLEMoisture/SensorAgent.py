'''
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
 Code to read a moisture sensor attached to a RedBear BLE Nano, then augment and format as JSON to send via socket connection to a gateway.
 Example of sending hydrology data to Microsoft Azure and analyzing with Azure Stream Analytics or Azure Machine Learning.
 
'''
import sys
import socket
import time
import datetime
import re

from BLEMoistureSensor import BLEMoistureSensor

Debug = False

Org      = "Your organization";
Disp     = "Sensor display name" 				       # will be the label for the curve on the chart
GUID     = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # ensures all the data from this sensor appears on the same chart. You can use the Tools/Create GUID in Visual Studio to create.
            						   # The last 6 bytes will be replaced with the mac address of the BLE module that is transmitting the moisture data.
Locn     = "Sensor location";

Vendor   = 0xfffe                                  # Vendor ID for our custom device
Product  = 0xfffe                                  # Product ID for our custom device

HOST     = '127.0.0.1'   
PORT     = 5002

CONNECT_RETRY_INTERVAL = 2
EXCEPTION_THRESHOLD    = 3
SEND_INTERVAL          = 5

s = None
		
def processSensorData( macAddress, value ) :
        global s
	timeStr = datetime.datetime.utcnow().isoformat()
		
	# replace last group of digits with mac address of BLE sensor board
	deviceID = GUID
	deviceID = deviceID[:24] + macAddress
		
	JSONString = "{"
	JSONString += "\"value\":" 				+ value
	JSONString += ",\"guid\":\"" 			+ deviceID
	JSONString += "\",\"organization\":\"" 	+ Org
	JSONString += "\",\"displayname\":\"" 	+ Disp
	JSONString += "\",\"unitofmeasure\":\"" + "vol/vol"
	JSONString += "\",\"measurename\":\"" 	+ "WaterContent"
	JSONString += "\",\"location\":\"" 		+ Locn
	JSONString += "\",\"timecreated\":\"" 	+ timeStr + "\""
	JSONString += "}"

	if Debug == True:
		print "JSONString=", JSONString

	if s != None :
		s.send("<" + JSONString + ">");         # sends to gateway over socket interface
		
def main() :
	try:
                global s
		# setup moisture sensor
		moistureSensor = BLEMoistureSensor()
		moistureSensor.setSensorDataAvailableEvent(processSensorData)

		# setup server socket
		if Debug == False :
			s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			print("Socket created.")
			while True:
				try:
					s.connect((HOST, PORT));
					break;
				except socket.error as msg:
					print("Socket connection failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
					time.sleep(CONNECT_RETRY_INTERVAL)
                        print ("Socket connection succeeded.")

		# this will listen forever for advertising events and call processSensorData() when data arrives
		moistureSensor.Listen();

	except KeyboardInterrupt: 
		print("Continuous polling stopped")
	        
if __name__ == '__main__':
    main()
