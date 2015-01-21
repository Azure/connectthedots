import sys
import usb.core
import socket
import time

HOST = ''   
PORT = 5000
 
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print 'Socket created'
 
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print 'Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1]
    sys.exit()
     
print 'Socket bind complete'
 
s.listen(10)
# print 'Socket now listening'

dev = usb.core.find(idVendor=0x16c0, idProduct=0x5dc)
assert dev is not None
print dev
print hex(dev.idVendor) + ', ' + hex(dev.idProduct)
 
conn, addr = s.accept()
while 1:
	ret = dev.ctrl_transfer(0xC0, 4, 0, 0, 200)
	dB = (ret[0] + ((ret[1] & 3) * 256)) * 0.1 + 30
	JSONdB="{\"dspl\":\"IMML Sound Level Meter 01\",\"Subject\":\"sound\",\"DeviceGUID\":\"81E79059-A393-4797-8A7E-526C3EF9D64B\",\"decibels\":" +str(dB) + "}"
	conn.send("<" + JSONdB + ">");
	print JSONdB
	time.sleep(1)
s.close()
