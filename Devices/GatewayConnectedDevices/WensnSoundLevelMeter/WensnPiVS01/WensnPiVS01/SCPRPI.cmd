@echo off
set puttydir="C:\Software\PuTTY\"
set prjdir=..\
rem set rpi_ip=raspberrypi
set rpi_ip=192.168.1.112
set rpi_usr=pi
set rpi_pw=raspberry


echo Copying Gateway and logging files
%puttydir%pscp -pw %rpi_pw% %prjdir%WensnSoundLevelMeter\WensnPiVS01.py %rpi_usr%@%rpi_ip%:RaspberryPiGateway/
