rem @echo off
set puttydir="C:\software\PuTTY\"
set prjdir=..\..\
set rpi_ip=10.121.204.248
set rpi_usr=pi
set rpi_pw=raspberry
set Configuration=Debug
set GW_Home=GatewayService
set Staging=%GW_Home%/Staging
set PUTTY_CMD=%puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% 
set PSCP_CMD=%puttydir%pscp -pw %rpi_pw% 

echo Creating GatewayService directory
del /f %temp%\gatewayservicemkdir.tmp
echo rm -rf %Staging%  >> %temp%\gatewayservicemkdir.tmp
echo rm -rf %GW_Home%  >> %temp%\gatewayservicemkdir.tmp
echo mkdir  %GW_Home%  >> %temp%\gatewayservicemkdir.tmp
echo mkdir  %Staging%  >> %temp%\gatewayservicemkdir.tmp
%PUTTY_CMD% -m %temp%\gatewayservicemkdir.tmp

echo Copying Gateway files
%PSCP_CMD% %prjdir%WindowsService\bin\%Configuration%\*.* %rpi_usr%@%rpi_ip%:%Staging%/

%PSCP_CMD% %prjdir%DeviceAdapters\SerialPort\bin\%Configuration%\Microsoft.ConnectTheDots.SerialPortAdapter.dll %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%DeviceAdapters\Socket\bin\%Configuration%\Microsoft.ConnectTheDots.SocketAdapter.dll         %rpi_usr%@%rpi_ip%:%Staging%/

echo copying scripts
%PSCP_CMD% %prjdir%Scripts\RaspberryPi\autorun.sh %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%Scripts\RaspberryPi\runonce.sh %rpi_usr%@%rpi_ip%:%Staging%/

echo Marking autorun.sh as executable
del /f %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/runonce.sh   >> %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorun.sh >> %temp%\rpigatewayautorunx.tmp
echo dos2unix %Staging%/runonce.sh    >> %temp%\rpigatewayautorunx.tmp
echo dos2unix %Staging%/autorun.sh  >> %temp%\rpigatewayautorunx.tmp
%PUTTY_CMD% -m %temp%\rpigatewayautorunx.tmp
