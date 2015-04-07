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
* the VI text editor comes by default on the Yocto image for the Intel Edison, but if you prefer Nano, connect with SSH and type the following commands:

                 opkg install http://repo.opkg.net/edison/repo/core2-32/ncurses-terminfo_5.9-r15.1_core2-32.ipk
                 opkg install http://repo.opkg.net/edison/repo/core2-32/nano_2.2.5-r3.0_core2-32.ipk

* To connect with the TI SensorTag, you need to enable Bluetooth low energy on the Intel Edison. Connect to the baord via SSH and type the following commands:
    * edit the file /etc/opkg/base-feeds.conf using vi or nano (see above for installing nano) and add the following lines:

                 src/gz all http://repo.opkg.net/edison/repo/all
                 src/gz edison http://repo.opkg.net/edison/repo/edison
                 src/gz core2-32 http://repo.opkg.net/edison/repo/core2-32

    * back in SSH, type the following commands:

                 opkg update
				 opkg install bluez5-dev

	* To activate bluetooth low energy, you then have to type the following commands in SSH:

                 rfkill unblock bluetooth
                 hciconfig hci0 up

##Prepare settings files##
You will need to edit the settings file of the application before deploying the application on the board to apply your own connection information for your Azure Event Hub.

* Open the file connectthedots\Devices\DirectlyConnectedDevices\NodeJS\IntelEdison\settings.json and edit the settings based on the configuration of your ehdevices Event Hub

                 "namespace": "{namespace}",
                 "keyname": "{key-name}",
                 "key": "{key}",
                 "eventhubname": "ehdevices",
                 "guid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
                 "displayname": "Edison",
                 "organization": "My Org",
                 "location":  "My location"

##Deploy the app##

The deployment is done using the deploy.cmd file found in the scripts subfolder:

* Edit the deploy.cmd file and enter your Edison's IP address as well as the location on your machine of the PuTTY tools.
* Start the deploy.cmd script in a cmd shell. This will copy the app files into the /node_app_slot folder on the device and will update the node packages required for the app to run.
* Once the files are copied in the /node_app_slot, the app will start automatically at reboot. You can simply reboot the board to start sending data Azure!


