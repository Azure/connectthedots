@echo off
set puttydir="C:\Software\PuTTY\"
set prjdir=..\
rem set rpi_ip=raspberrypi
set rpi_ip=10.121.204.186
set rpi_usr=pi
set rpi_pw=raspberry


echo Copying Gateway and logging files
%puttydir%pscp -pw %rpi_pw% WensnPiVS01.py %rpi_usr%@%rpi_ip%:SensorService/
