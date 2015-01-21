
@echo off
set puttydir="C:\software\PuTTY\"
set prjdir=..\
rem set rpi_ip=raspberrypi
set rpi_ip=10.121.204.204
set rpi_usr=pi
set rpi_pw=raspberry

echo Copying log files
%puttydir%pscp %rpi_usr%@%rpi_ip%:/home/pi/RaspberryPiGateway/logs/*.log ..\bin\release\logs\
