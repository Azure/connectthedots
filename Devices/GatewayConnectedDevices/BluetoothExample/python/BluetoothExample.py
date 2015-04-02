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
import serial
import sys
import socket
import time
import datetime

#SensorSubject = "sound"                           # determines how Azure website will chart the data
Org      = "My organization";
Disp     = "Bluetooth example"                     # will be the label for the curve on the chart
GUID     = "nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn"  # ensures all the data from this sensor appears on the same chart. You can use the Tools/Create GUID in Visual Studio to create
Locn     = "here";
Measure  = "measure";
Units    = "units";

HOST = '127.0.0.1'   
PORT = 5000

SERIAL_PORT  = '/dev/ttyAMA0'
BAUD_RATE    = 9600

CONNECT_RETRY_INTERVAL = 2
EXCEPTION_THRESHOLD    = 3
SEND_INTERVAL          = 1

while True:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    print("Socket created")

    while True:
        try:
            s.connect((HOST, PORT));
            break;
        except socket.error as msg:
            print("Socket connection failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
            time.sleep(CONNECT_RETRY_INTERVAL)
    print ("Socket connection succeeded.")
    
    exceptions_count = 0
    serial_port = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=.1)
    while True:
        timeStr = datetime.datetime.utcnow().isoformat()
        try:
            serialData = serial_port.readline()[:-2]
            if len(serialData) > 0:
                JSONdata="{\"value\":"+serialData+",\"guid\":\""+GUID+"\",\"organization\":\""+Org+"\",\"displayname\":\""+Disp +"\",\"unitofmeasure\":\""+Units+"\",\"measurename\":\""+Measure+"\",\"location\":\""+Locn+"\",\"timecreated\":\""+timeStr+"\"}"
                s.send("<" + JSONdata + ">");                  # sends to gateway over socket interface
                print(JSONdata)                                # print only for debugging purposes
        except Exception as msg:
            exceptions_count += 1
            print(msg[0])
            # if we get too many exceptions, we assume the server is dead
            # we will ignore the casual exception
            if exceptions_count > EXCEPTION_THRESHOLD:
                break 
            else:
                continue
                    
        time.sleep(1)
        
    # will never get here, unless server dies         
    try: 
        s.close()
    except Exception as msg:
        # eat all exception and go back to connect loop 
        print(msg[0])
