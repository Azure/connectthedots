set puttydir=d:\software\putty\
set prjdir=..\
set rpi_ip=192.168.1.107
set rpi_usr=pi
set rpi_pw=raspberry
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Debug\CloudPI.exe %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Debug\Amqp.Net.dll %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Debug\Newtonsoft.Json.dll %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\autorun.sh %rpi_usr%@%rpi_ip%:RaspberryPiGateway/autorun.sh
