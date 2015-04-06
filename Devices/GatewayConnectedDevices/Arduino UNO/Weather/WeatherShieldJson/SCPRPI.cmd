@echo off
set puttydir="C:\Software\PuTTY\"
set prjdir=..\
rem set rpi_ip=raspberrypi
set rpi_ip=10.121.204.248
set rpi_usr=pi
set rpi_pw=raspberry


echo Copying Gateway and logging files
rem %puttydir%pscp -pw %rpi_pw% %prjdir%WensnPiVS01\WensnPiVS01.py %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
%puttydir%pscp -pw %rpi_pw% WeatherShieldJson.ino %rpi_usr%@%rpi_ip%:GatewayService/
