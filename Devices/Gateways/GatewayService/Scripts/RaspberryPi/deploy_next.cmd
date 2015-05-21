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

set puttydir="C:\Program Files (x86)\PuTTY\"
set prjdir=..\..\
set scdir=%prjdir%Scripts\ScriptConverter\bin\
set rpi_ip=192.168.1.213
set rpi_usr=pi
set rpi_pw=raspberry
set Configuration=Debug
set GW_Home=ctdgtwy
set Staging=%GW_Home%/staging
set PUTTY_CMD=%puttydir%putty %rpi_usr%@%rpi_ip% -pw %rpi_pw% 
set PSCP_CMD=%puttydir%pscp -pw %rpi_pw% 
set PYTHON_SCRIPTS_DIR=..\..\..\..\GatewayConnectedDevices\
set BT_PYTHON_SCRIPT_DIR=%PYTHON_SCRIPTS_DIR%BluetoothUARTExample\
set BT_USB_PYTHON_SCRIPT_DIR=%PYTHON_SCRIPTS_DIR%BtUSB_2_BtUART_Example\
set WENSN_PYTHON_SCRIPT_DIR=%PYTHON_SCRIPTS_DIR%WensnSoundLevelMeter\WensnPiVS01\
echo editing line endings for Pi
%scdir%%Configuration%\ScriptConverter.exe "autorunWensnSoundSensor.sh" 
%scdir%%Configuration%\ScriptConverter.exe "autorunUartBT.sh" 
%scdir%%Configuration%\ScriptConverter.exe "autorunUart2UsbBt.sh" 

echo Copying file that starts up python script to read USB port connected to Wensn and format as JSON
%PSCP_CMD% %WENSN_PYTHON_SCRIPT_DIR%WensnPiVS01.py  %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% Modified\autorunWensnSoundSensor.sh  %rpi_usr%@%rpi_ip%:%Staging%/

echo Copying file that starts up python script to read UART port connected to BT and format as JSON
%PSCP_CMD% %BT_PYTHON_SCRIPT_DIR%BluetoothUARTExample.py  %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %BT_PYTHON_SCRIPT_DIR%SetupSerialBaudRate.py  %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% Modified\autorunUartBT.sh  %rpi_usr%@%rpi_ip%:%Staging%/

echo Copying file that starts up python script to read USB BT module and format as JSON
%PSCP_CMD% %BT_USB_PYTHON_SCRIPT_DIR%BtUSB_2_BtUART_Example.py  %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% %BT_USB_PYTHON_SCRIPT_DIR%TestServer.py  %rpi_usr%@%rpi_ip%:%Staging%/
%PSCP_CMD% Modified\autorunUart2UsbBt.sh  %rpi_usr%@%rpi_ip%:%Staging%/

echo Marking autorunWensnSoundSensor.sh and autorunUartBT.sh as executables
del /f %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorunWensnSoundSensor.sh     >> %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorunUartBT.sh               >> %temp%\rpigatewayautorunx.tmp
echo chmod 755 %Staging%/autorunUart2UsbBt.sh           >> %temp%\rpigatewayautorunx.tmp
%PUTTY_CMD% -m                                    %temp%\rpigatewayautorunx.tmp