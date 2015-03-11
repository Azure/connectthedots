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

export GW_HOME=~/GatewayService
export LOGS=$GW_HOME/logs
export STAGING=$GW_HOME/Staging

# Kill all mono processes and GatewayService as well, kill the monitoring process that is performing a sleep
echo "Trying to kill all mono processes..."
for KILLPID in `ps axo pid,ppid,cmd | grep -i 'mono'           | awk '{ print $1;}'`; do sudo kill -9 $KILLPID; done
for KILLPID in `ps axo pid,ppid,cmd | grep -i 'gatewayservice' | awk '{ print $1;}'`; do sudo kill -9 $KILLPID; done
for KILLPID in `ps axo pid,ppid,cmd | grep -i 'sleep'          | awk '{ print $2;}'`; do sudo kill -9 $KILLPID; done

echo "Trying to delete lock file if there is any..."
sudo rm -f /tmp/Microsoft.ConnectTheDots.GatewayService.exe.lock

# move all files from Staging GW_HOME to runtime folder, delete logs
echo updating files
rm -rf $LOGS/*
mkdir $LOGS
find $GW_HOME/ -maxdepth 1 -type f -delete
cp $STAGING/* $GW_HOME/
rm $GW_HOME/autorun.sh
mv $GW_HOME/autorun_install.sh $GW_HOME/autorun.sh

echo "Starting host processes..."
#
# event log entries will be written to /var/lib/mono/EventLog/Application
#
echo "Setting MONO_EVENTLOG_TYPE to local"
export MONO_EVENTLOG_TYPE=local
#
echo "Starting Gateway"
cd $GW_HOME
#MONO_LOG_LEVEL=debug /usr/bin/mono-service $GW_HOME/Microsoft.ConnectTheDots.GatewayService.exe --debug > monoOutput.txt &
#/usr/bin/mono-service $GW_HOME/Microsoft.ConnectTheDots.GatewayService.exe
$GW_HOME/autorun.sh &

echo "Starting supplementary sensor script if present"
$GW_HOME/autorun2.sh &

#
# Add the below line to /etc/rc.local
#
#   export GW_HOME=~/GatewayService
#   $GW_HOME/autorun.sh &
#
# and don't forget to make autorun.sh executable (sudo chmod 755 autorun)
