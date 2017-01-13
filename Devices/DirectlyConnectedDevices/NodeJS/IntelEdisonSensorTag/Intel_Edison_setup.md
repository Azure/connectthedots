This document explains how to set up an Intel Edison board to send data to Azure Event Hub using the REST interface. 
It assumes that you have the right tools installed and that you have cloned or downloaded the ConnectTheDots.io project on your machine.

##Hardware requirements ##
Check out the hardware requirements [here](hardware.md).

##Prerequisites ##

To deploy the application you will need the following:

* For Windows, download PuTTY and PSCP from [here](http://www.putty.org/).
* WiFi Internet access for the device.

To work on the code of the project, you can use your favorite editor... or leverage the Visual Studio 2013 support for node.js development, in which case, you will need:

* Visual Studio 2013 [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) or above.
* [Node.js tools for Visual Studio](https://nodejstools.codeplex.com/)

## Configure the Edison##

* Follow the instructions on the [Intel support page](https://communities.intel.com/docs/DOC-23192) to setup the Yocto image on the Edison board.
* Connect to the Intel Edison from your laptop, via a USB using PuTTY (or your favorite SSH tool)
    * For Windows, you can download PuTTY and PSCP from [here](http://www.putty.org/).
    * Connect to the Pi using the IP address of the Pi.
* Via SSH you can setup the WiFi connection typing:
                
                 configure_edison --wifi

* Once WiFi is setup you can connect your SSH tool through the network using the IP address displayed when doing the setup at previous step.
* First thing you will need to do is to update the links to opkg packages in order to install a few libraries. In SSH console, type the following commands:

                 echo "src/gz all http://repo.opkg.net/edison/repo/all" >> /etc/opkg/base-feeds.conf
                 echo "src/gz edison http://repo.opkg.net/edison/repo/edison" >> /etc/opkg/base-feeds.conf
                 echo "src/gz core2-32 http://repo.opkg.net/edison/repo/core2-32" >> /etc/opkg/base-feeds.conf

                 opkg update

* the VI text editor comes by default on the Yocto image for the Intel Edison, but if you prefer Nano, connect with SSH and type the following commands:
                 opkg install nano

* To connect with the TI SensorTag, you need to enable Bluetooth low energy on the Intel Edison. Connect to the baord via SSH and type the following commands:

    * In SSH, type the following commands:

				 opkg install bluez5-dev

	* To activate bluetooth low energy, you then have to type the following commands in SSH:

                 rfkill unblock bluetooth
                 hciconfig hci0 up

##Setup the app on the board##

* In the remote terminal, type the following commands:

                 cd /node_app_slot
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/inteledisonsensortagctd.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/package.json
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/settings.json
                 mkdir lib
                 cd lib
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/lib/cc2540.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/lib/cc2650.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/lib/common.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/IntelEdisonSensorTag/lib/sensortag.js
                 npm install
                 
* Before running the app, you need to update the settings.json file to input the device's connetion string and a unique device id.
Following the instructions [here](../../../readme.md), get the connection string for your device.
                 
* In the remote terminal, open the file settings.json and edit the settings based on the configuration of your ehdevices Event Hub (if you want to use nano, just type nano settings.json and once your edits are done, type CTRL+X then Y to save). Note that the device id shall be unique per device so that data is not messed up in the connectthedots portal.

                 "iothubconnectionstring": "<connectionstring>",
                 "deviceid": "<deviceid>",
                 "displayname": "EdisonSensortag",
                 "organization": "My Org",
                 "location":  "My location"
                 
##Run the app##

You can run the app manually typing the following in the remote terminal:

                 node .
                 
Because the application is deployed in the node_app_slot on the Intel Edison, it will start automatically at boot time.
