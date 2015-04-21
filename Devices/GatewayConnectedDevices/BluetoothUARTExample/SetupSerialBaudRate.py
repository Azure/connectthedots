'''
 Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.

 The MIT License (MIT)

 Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
  
 -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= 
This script is used for setting up raspberry's uart.
It corrects some config files.
User can set baudrate as argument.
'''

import os
import sys

def fixEtcInittab(baudrate):
    lines = []
    inittabFilename = '/etc/inittab'
    inittabTrueLine = '#T0:23:respawn:/sbin/getty -L ttyAMA0 %d vt100\n' % baudrate

    with open(inittabFilename, 'r') as fd:
        for line in fd:
            lines.append(line)
    fd.close()

    for i, line in enumerate(lines):
        if line.startswith('T0:23:respawn') and line.endswith('vt100\n'):
            lines[i] = inittabTrueLine
            os.remove(inittabFilename)
            
    fd = open(inittabFilename, 'w') 
    for i, line in enumerate(lines):
        fd.write(lines[i])
        
    fd.close()  
    #print lines
    
    
def fixBootCmdLine(baudrate):
    lines = []
    bootcmdlineFilename = '/boot/cmdline.txt'
    bootcmdlineTrueLine = 'dwc_otg.lpm_enable=0 console=ttyAMA0,%d ' % baudrate \
                            + 'kgdboc=ttyAMA0,%d console=tty1 root=/dev/mmcblk0p2 ' % baudrate \
                            + 'rootfstype=ext4 rootwait\n'
        
    with open(bootcmdlineFilename, 'r') as fd:
        for line in fd:
            lines.append(line)
    fd.close()
    
    for i, line in enumerate(lines):
        if line.startswith('dwc_otg.lpm_enable=0') and line.endswith('rootwait\n'):
            lines[i] = bootcmdlineTrueLine
            os.remove(bootcmdlineFilename)
            
    fd = open(bootcmdlineFilename, 'w') 
    for i, line in enumerate(lines):
        fd.write(lines[i])
        
    fd.close()  
    #print lines
    
def main(arguments):
    baudrate = int(arguments[1])
    #print baudrate
    fixEtcInittab(baudrate)
    fixBootCmdLine(baudrate)
    

if __name__ == '__main__':
    main(sys.argv)
