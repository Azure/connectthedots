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
 Code to read data from a Wensn 1361 Digital Sound Level Meter, then augment and format as JSON to send via socket connection to a gateway.
 Example of sending sound level data to Microsoft Azure and analyzing with Azure Stream Analytics or Azure Machine Learning.
 Real time output viewable at http://connectthedots.msopentech.com .
'''
import sys
import usb.core
import socket
import time

SensorSubject = "sound"                                 # determines how Azure website will chart the data
DeviceDisplayName = "Wensn SLM 01"                      # will be the label for the curve on the chart
DeviceGUID = "92B6F1F2-EA6B-4A72-8B8A-3720AF242D13"     # ensures all the data from this sensor appears on the same chart. You can use the Tools/Create GUID in Visual Studio to create
Vendor = 0x16c0                                         # Vendor ID for Wensn
Product=0x5dc                                           # Product ID for Wensn 1361

HOST = ''   
PORT = 5000
 
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print("Socket created")

try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print("Bind failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
    sys.exit()
     
print ("Socket bind complete")
 
s.listen(10)
# print("Socket now listening")

# dev = usb.core.find(idVendor=0x16c0, idProduct=0x5dc)
dev = usb.core.find(idVendor=Vendor, idProduct=Product)
assert dev is not None
print(dev)
print(hex(dev.idVendor) + ", " + hex(dev.idProduct))
 
conn, addr = s.accept()
while 1:
	ret = dev.ctrl_transfer(0xC0, 4, 0, 0, 200)
	dB = (ret[0] + ((ret[1] & 3) * 256)) * 0.1 + 30
	timeStr = datetime.datetime.utcnow().isoformat()
	try:
		JSONdB="{\"value\":"+str(dB)+",\"guid\":"+DeviceGUID+",\"organization\":\"contoso\",\"displayname\":\""+DeviceDisplayName +"\",\"unitofmeasure\":\"decibels\",\"measurename\":\"sound\",\"location\":\"here\",\"timecreated\":"+timeStr+"}"
		conn.send("<" + JSONdB + ">");                  # sends to gateway over socket interface
		print(JSONdB)                                   # print only for debugging purposes
	except Exception as msg:
		print(msg[1])

	time.sleep(1)
s.close()
