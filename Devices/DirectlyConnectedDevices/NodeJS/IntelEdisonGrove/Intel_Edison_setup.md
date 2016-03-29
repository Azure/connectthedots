This document explains how to set up an Intel Edison board to send data to AzureIoT services using the REST interface. 
It assumes that you have already deployed a connectthedots solution following the instructions [here](../../../../Azure/ARMTemplate/Readme.md).

##Hardware requirements ##
Check out the hardware requirements [here](hardware.md).
Connect the sensors as follows on the Grove Base shield (itself plugged onto the Intel Edison Arduino brakout board). Note that you can change the connections, but will have to adapt the code if you do so.

Grove Light Sensor           -   A0
Grove Moisture Sensor        -   A1
Grove Rotary potentiometer   -   A2
Grove UV Sensor              -   A3
Grove LCD RGB Backlight      -  I2C
Grove Temperature&Humidity   -  I2C
Grove PIR motion Sensor      -   D7
Grove Buzzer                 -   D4
Grove Button                 -   D6

Note that all sensors are not required to run the sample. Just edit inteledisonctd.js to comment the lines related to the sensor you don't have.

##Prerequisites##

To deploy the application you will need the following:

* For Windows, download PuTTY from [here](http://www.putty.org/).
* WiFi Internet access for the device.

To work on the code of the project, you can use your favorite editor.

##Configure the Edison##

* Follow the instructions on the [Intel support page](https://communities.intel.com/docs/DOC-23192) to setup the Yocto image on the Edison board.
* Connect to the Intel Edison from your laptop, via a USB using PuTTY (or your favorite remote terminal tool)

* In the remote terminal, you can setup the WiFi connection typing:
                
                 configure_edison --wifi

* Once WiFi is setup you can connect your SSH tool through the network using the IP address displayed when doing the setup at previous step or using the command ifconfig.
* the VI text editor comes by default on the Yocto image for the Intel Edison, but if you prefer Nano, connect with SSH and type the following commands:

                 echo "src/gz all http://repo.opkg.net/edison/repo/all" >> /etc/opkg/base-feeds.conf
                 echo "src/gz edison http://repo.opkg.net/edison/repo/edison" >> /etc/opkg/base-feeds.conf
                 echo "src/gz core2-32 http://repo.opkg.net/edison/repo/core2-32" >> /etc/opkg/base-feeds.conf

                 Update your package cache

                 opkg update

                 opkg install nano

##Setup the app on the board##

* In the remote terminal, type the following commands:

                 cd /node_app_slot
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonGrove/inteledisonctd.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonGrove/package.json
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonGrove/settings.json
                 npm install
                 
* Before running the app, you need to update the settings.json file to input the device's connetion string and a unique device id.
Following the instructions [here](../../../DeviceSetup.md), get the connection string for your device.
                 
* In the remote terminal, open the file settings.json and edit the settings based on the configuration of your ehdevices Event Hub (if you want to use nano, just type nano settings.json and once your edits are done, type CTRL+X then Y to save). Note that the device id shall be unique per device so that data is not messed up in the connectthedots portal.

                 "iothubconnectionstring": "<connectionstring>",
                 "deviceid": "<deviceid>",
                 "displayname": "Edison",
                 "organization": "My Org",
                 "location":  "My location"
                 
##Run the app##

You can run the app manually typing the following in the remote terminal:

                 node .
                 
Because the application is deployed in the node_app_slot on the Intel Edison, it will start automatically at boot time.
             
