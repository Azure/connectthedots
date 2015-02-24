@echo off
set puttydir="C:\software\PuTTY\"
set prjdir=..\
set rpi_ip=<your Raspberry PI address here>
set rpi_usr=<your Raspberry PI user name>
set rpi_pw=<your Raspberry PI password>

rem echo Creating GatewayService directory
rem echo mkdir GatewayService > %temp%\gatewayservicemkdir.tmp
rem %puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% -m %temp%\gatewayservicemkdir.tmp

echo Copying Gateway files
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\Amqp.Net.dll %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\Gateway.dll %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\GatewayService.exe %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\GatewayService.exe.config %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\Newtonsoft.Json.dll %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%WindowsService\bin\Release\SharedInterfaces.dll %rpi_usr%@%rpi_ip%:GatewayService/

%puttydir%pscp -pw %rpi_pw% %prjdir%DataIntakes\SerialPortListener\bin\Release\SerialPortListener.dll %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%DataIntakes\SocketListener\bin\Release\SocketListener.dll %rpi_usr%@%rpi_ip%:GatewayService/

echo copying scripts
rem %puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\autorun.sh %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\startall.sh %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\killall.sh %rpi_usr%@%rpi_ip%:GatewayService/
%puttydir%pscp -pw %rpi_pw% %prjdir%Scripts\fixall.sh %rpi_usr%@%rpi_ip%:GatewayService/

echo Marking autorun.sh as executable
rem echo chmod 755 GatewayService/autorun.sh > %temp%\rpigatewayautorunx.tmp
rem %puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% -m %temp%\rpigatewayautorunx.tmp
