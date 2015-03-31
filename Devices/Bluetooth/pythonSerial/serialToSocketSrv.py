import sys
import socket

HOST = '127.0.0.1'
PORT = 5001
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print("Bind failed. Error Code : " + str(msg[0]) + " Message " + msg[1])
    sys.exit()
print ("Socket bind complete")
s.listen(10)
conn, addr = s.accept()
while True:
    data = conn.recv(1024)
    if len(data) > 0:
        strData = data.decode('utf-8')
        print strData
s.close()
