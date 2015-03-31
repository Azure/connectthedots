import serial
import sys
import socket
import time
import datetime

HOST = '127.0.0.1'   
PORT = 5001
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
    serial_port = serial.Serial('/dev/ttyAMA0', 9600, timeout=.1)
    while True:
        timeStr = datetime.datetime.utcnow().isoformat()
        try:
            serialData = serial_port.readline()[:-2]
            if len(serialData) > 0:
                s.send(serialData);                  # sends to gateway over socket interface
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
