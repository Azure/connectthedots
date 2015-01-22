@echo off
set puttydir="C:\software\PuTTY\"
set prjdir=..\
set rpi_ip=<your Raspberry PI address here>
set rpi_usr=<ytour sensor addres here>
set rpi_pw=<password for PuTTY>

rem echo Creating RaspberryPiGateway directory
rem echo mkdir RaspberryPiGateway > %temp%\rpigatewaymkdir.tmp
rem %puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% -m %temp%\rpigatewaymkdir.tmp

echo Copying Gateway and logging files
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\RaspberryPiGateway.exe %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\Newtonsoft.Json.dll %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\NLog.config %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\RaspberryPiGateway.exe.config %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\NLog.dll %rpi_usr%@%rpi_ip%:RaspberryPiGateway/

rem %puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\autorun.sh %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\startall.sh %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\killall.sh %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\fixall.sh %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\SoundLevelMeter\usbsplsocket.py %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% %prjdir%bin\Release\Amqp.Net.dll %rpi_usr%@%rpi_ip%:RaspberryPiGateway/

echo Marking autorun.sh as executable
rem echo chmod 755 RaspberryPiGateway/autorun.sh > %temp%\rpigatewayautorunx.tmp
rem %puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% -m %temp%\rpigatewayautorunx.tmp
