#  ---------------------------------------------------------------------------------
#  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
# 
#  The MIT License (MIT)
# 
#  Permission is hereby granted, free of charge, to any person obtaining a copy
#  of this software and associated documentation files (the "Software"), to deal
#  in the Software without restriction, including without limitation the rights
#  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
#  copies of the Software, and to permit persons to whom the Software is
#  furnished to do so, subject to the following conditions:
# 
#  The above copyright notice and this permission notice shall be included in
#  all copies or substantial portions of the Software.
# 
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
#  THE SOFTWARE.
#  ---------------------------------------------------------------------------------
#!/bin/bash
echo "Starting host processes"
#
echo "Killing mono"
sudo pkill mono
#
echo "Trying to delete lock file if there is any"
sudo rm /tmp/GatewayService.exe.lock
#
# event log entries will be written to /var/lib/mono/EventLog/Application
echo "Setting MONO_EVENTLOG_TYPE to local"
export MONO_EVENTLOG_TYPE=local
#
echo "Starting Gateway"
sudo MONO_LOG_LEVEL=debug /usr/bin/mono-service /home/pi/GatewayService/GatewayService.exe --debug > monoOutput.txt &
#
# Add the below line to /etc/rc.local
#   /home/pi/GatewayService/autorun.sh &
# and don't forget to make autorun.sh executable (sudo chmod 755 autorun)
