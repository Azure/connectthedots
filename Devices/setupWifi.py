# https://learn.adafruit.com/adafruits-raspberry-pi-lesson-3-network-setup/setting-up-wifi-with-occidentalis
import sys

interfacesFile = '/etc/network/interfaces'
allowHotPlug = 'allow-hotplug wlan0\n'
autoWlan = 'auto wlan0\n'
wlanDhcp = 'iface wlan0 inet dhcp\n'

def main(argv):
    print 'Please run this script with sudo!'
    if (len(argv) < 3):
        print 'Not enough args, please provide ssid and password for wifi'
        return
    
    ssid = argv[1]
    psk = argv[2]
    wpaSsidStr = '\twpa-ssid ' + '\"' + ssid + '\"' + '\n'
    wpaPskStr = '\twpa-psk ' + '\"' + psk + '\"' + '\n'
    
    interfacesFd = open(interfacesFile, "a")
    interfacesFd.write(allowHotPlug)
    interfacesFd.write(autoWlan)
    interfacesFd.write('\n')
    interfacesFd.write(wlanDhcp)
    interfacesFd.write(wpaSsidStr)
    interfacesFd.write(wpaPskStr)
    interfacesFd.close()
    
    print 'Done!'

if __name__ == "__main__":
    main(sys.argv)