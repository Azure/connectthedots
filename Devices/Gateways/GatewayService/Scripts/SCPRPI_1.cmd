@echo off
set puttydir="C:\software\PuTTY\"
set prjdir=..\
set rpi_ip=<your Raspberry PI address here>
set rpi_usr=<your Raspberry PI user name>
set rpi_pw=<your Raspberry PI password>
set Configuration=Debug
set GW_Home=GatewayService
set Staging=%GW_Home%/Staging
set PUTTY_CMD=%puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% 
set PSCP_CMD=%puttydir%pscp -pw %rpi_pw% 

echo Creating GatewayService directory
echo rm -rf %Staging%  > %temp%\gatewayservicemkdir.tmp
echo mkdir  %GW_Home%  > %temp%\gatewayservicemkdir.tmp
echo mkdir  %Staging%  > %temp%\gatewayservicemkdir.tmp
%PUTTY_CMD% -m %temp%\gatewayservicemkdir.tmp

echo Copying Gateway files
%PSCP_CMD% %prjdir%WindowsService\bin\%Configuration%\*.* %rpi_usr%@%rpi_ip%:%Staging%/

%PSCP_CMD% %prjdir%DataIntakes\SerialPortListener\bin\%Configuration%\SerialPortListener.dll %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%DataIntakes\SocketListener\bin\%Configuration%\SocketListener.dll         %rpi_usr%@%rpi_ip%:%Staging%/

echo copying scripts
%PSCP_CMD% %prjdir%Scripts\autorun.sh %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%Scripts\runonce.sh %rpi_usr%@%rpi_ip%:%Staging%/

echo Marking autorun.sh as executable
echo chmod 755 %Staging%/runonce.sh > %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorun.sh > %temp%\rpigatewayautorunx.tmp
%PUTTY_CMD% -m %temp%\rpigatewayautorunx.tmp
