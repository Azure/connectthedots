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

set puttydir="C:\software\PuTTY\"
set prjdir=..\
set rpi_ip=10.121.204.204
set rpi_usr=pi
set rpi_pw=raspberry

echo Copying log files
%puttydir%pscp %rpi_usr%@%rpi_ip%:/var/lib/mono/eventlog/Application/*.log ..\bin\release\logs\
