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
 Code to read data from a sensor which uses Bluetooth to transmit data, then augment and format as JSON to send via socket connection to a gateway.
 Example of sending data to Microsoft Azure and analyzing with Azure Stream Analytics or Azure Machine Learning.
 Real time output viewable at http://connectthedots.msopentech.com .
'''
import bluetooth
import sys
import socket
import time
import datetime

#SensorSubject = "distanceMeasurer"                           # determines how Azure website will chart the data
Org      = "My organization";
Disp     = "Bluetooth example"                     # will be the label for the curve on the chart
GUID     = "nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn"  # ensures all the data from this sensor appears on the same chart. You can use the Tools/Create GUID in Visual Studio to create
Locn     = "here";
Measure  = "measure";
Units    = "units";

HOST = '127.0.0.1'   
PORT = 5000

BT_PORT = 1
BT_PACKET_LEN = 2
BT_DEV_ADDR = "20:14:10:10:14:17"	# Please set needed MAC address
BT_SOCK_TIMEOUT = 10

CONNECT_RETRY_INTERVAL = 2

def connectSockets(bt, gatewaySock):
    # Connect BT first
    while bt == None:
        print "Connection RFCOMM"
        try:
            bt = bluetooth.BluetoothSocket(bluetooth.RFCOMM)
            bt.connect((BT_DEV_ADDR, BT_PORT));
            print ("BT connection succeded")
        except socket.error as msg:
            bt = None
            print("Socket connection failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
            time.sleep(CONNECT_RETRY_INTERVAL)
        
    while gatewaySock == None:
        print "Connecting TCP"
        try:
            gatewaySock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            gatewaySock.connect((HOST, PORT));
            print ("Connection to gateway succeded")
        except socket.error as msg:
            gatewaySock = None
            print("Socket connection failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
            time.sleep(CONNECT_RETRY_INTERVAL)
    return bt, gatewaySock

def recvDataFromBT(bt, packetLen):
    btData = 0
    i = 0
    while(i < packetLen):
        byteAsStr = bt.recv(1)
        if (byteAsStr == ''):
            break
        byte = ord(byteAsStr)
        btData = (btData << 8) + byte
        i = i + 1
    return str(btData)
    
bt = None
s = None
while True:
    bt, s = connectSockets(bt, s)

    btData = None
    # btData != "" means remote host is down
    while btData == None:
        wasExceptionOccured = 0
        try:
            btData = recvDataFromBT(bt, BT_PACKET_LEN);
        except socket.error as sockErr:
            print(sockErr)
            try: 
                s.close()
            except Exception as msg:
                print(msg[0])
            wasExceptionOccured = 1
        if (wasExceptionOccured == 1 or btData == ''):
            # something went wrong, reconnect bluetooth socket
            btData = None
            bt = None
            bt = connectSockets(bt,s)

        timeStr = datetime.datetime.utcnow().isoformat()
        JSONdata = "{\"value\":"+btData+",\"guid\":\""+GUID+"\",\"organization\":\""+Org+"\",\"displayname\":\""+Disp +"\",\"unitofmeasure\":\""+Units+"\",\"measurename\":\""+Measure+"\",\"location\":\""+Locn+"\",\"timecreated\":\""+timeStr+"\"}"
        print(JSONdata) 
        wasExceptionOccured = 0
        try:
            # send to gateway over socket interface
            bytesNeedToBeSent = len(JSONdata)
            bytesSent = 0
            while(bytesSent < bytesNeedToBeSent):
                bytesSent = bytesSent + s.send("<" + JSONdata + ">")
                
            # TODO check if all bytes sent. Sent again if necessary.
        except Exception as msg:
            print(msg[0])
            try: 
                s.close()
            except Exception as msg:
                print(msg[0])
            wasExceptionOccured = 1
            
        if (wasExceptionOccured == 1):
            # something went wrong, reconnect gateway socket
            s = None
            print "gateway socket exception occured"
            bt,s = connectSockets(bt,s)
                    
        time.sleep(1)
        
# will never get here, unless server dies         
try: 
    s.close()
except Exception as msg:
# eat all exception and go back to connect loop 
    print(msg[0])
try: 
    bt.close()
except Exception as msg:
    # eat all exception and go back to connect loop 
    print(msg[0])
