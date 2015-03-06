@echo off
REM //  ---------------------------------------------------------------------------------
REM //  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
REM // 
REM //  The MIT License (MIT)
REM // 
REM //  Permission is hereby granted, free of charge, to any person obtaining a copy
REM //  of this software and associated documentation files (the "Software"), to deal
REM //  in the Software without restriction, including without limitation the rights
REM //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
REM //  copies of the Software, and to permit persons to whom the Software is
REM //  furnished to do so, subject to the following conditions:
REM // 
REM //  The above copyright notice and this permission notice shall be included in
REM //  all copies or substantial portions of the Software.
REM // 
REM //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
REM //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
REM //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
REM //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
REM //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
REM //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
REM //  THE SOFTWARE.
REM //  ---------------------------------------------------------------------------------

set puttydir="E:\tools\Raspeberry\"
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
%PSCP_CMD% %prjdir%Scripts\RaspberryPi\autorun_once.sh    %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%Scripts\RaspberryPi\autorun_install.sh %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %prjdir%Scripts\RaspberryPi\runonce.sh         %rpi_usr%@%rpi_ip%:%Staging%/

echo Marking autorun_once.sh and autorun_install.sh as executable
del /f %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/runonce.sh            >> %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorun_once.sh       >> %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorun_install.sh    >> %temp%\rpigatewayautorunx.tmp
echo dos2unix  %Staging%/runonce.sh            >> %temp%\rpigatewayautorunx.tmp
echo dos2unix  %Staging%/autorun_once.sh       >> %temp%\rpigatewayautorunx.tmp
echo dos2unix  %Staging%/autorun_install.sh    >> %temp%\rpigatewayautorunx.tmp
%PUTTY_CMD% -m                                    %temp%\rpigatewayautorunx.tmp
