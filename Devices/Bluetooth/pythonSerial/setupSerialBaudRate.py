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
