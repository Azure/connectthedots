This document explains how to set up a BeagleBone Black board to send data to Azure IoT services Hub using the REST interface. 

##Hardware requirements ##
Check out the hardware requirements [here](hardware.md).

##Prerequisites ##

To deploy the application you will need the following:

* For Windows, download PuTTY and PSCP from [here](http://www.putty.org/).
* Wired Internet access for the device.

To work on the code of the project, you can use your favorite code editor. 

## Configure the BeagleBone Black##

* Connect the Grove Cape on the BeagleBone Black board
* Connect the light sensor on the connector J7 of the Grove Cape
* Connect the Temperature sensor on the connector J3 of the Grove Cape
* Follow the instructions on the [BeagleBone.org site](http://beagleboard.org/getting-started) to setup the board.
    * Update the image to the latest as instructed on the site  
* Connect to the BeagleBone Black from your laptop, via a USB cable and use PuTTY (or your favorite remote terminal tool) to login to the OS (default user is "root" with no pasword)
    
The default BBB image comes with node.js preinstalled.

##Setup the app on the board##

* In the remote terminal, type the following commands:

                 mkdir node_app_slot
                 cd node_app_slot
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/BeagleBoneBlack/beagleboneblackctd.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/BeagleBoneBlack/package.json
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/BeagleBoneBlack/settings.json
                 npm install
                 
* Before running the app, you need to update the settings.json file to input the device's connetion string and a unique device id.
Following the instructions [here](../../../readme.md), get the connection string for your device.
                 
* In the remote terminal, open the file settings.json and edit the settings based on the configuration of your ehdevices Event Hub (if you want to use nano, just type nano settings.json and once your edits are done, type CTRL+X then Y to save). Note that the device id shall be unique per device so that data is not messed up in the connectthedots portal.

                 "iothubconnectionstring": "<connectionstring>",
                 "deviceid": "<deviceid>",
                 "displayname": "BeagleBoneBlack",
                 "organization": "My Org",
                 "location":  "My location"

* Once you have changed the settings file, you can test, you can test the app by typing the following command in the remote terminal (you need to be in the /node_app_slot folder):

        node .

##Setup the app to start automatically at boot##
 In order to have the application start automatically at boot, you need to modify the startup script rc.local.
 In the sample we are using forever, a very convinient node tool that starts a node app as a daemon and keeps it alive

* Type the following commands in the remote terminal:

        npm install -g forever
        nano /etc/rc.local
    
* In nano, edit the rc.local file by adding the following lines just before the "exit 0" line

        /usr/local/bin/forever start -a -f --spinSleepTime 5000 /root/node_app_slot/beagleboneblackctd.js
    
* To save the file, press CTRL + X then Y and ENTER
* You can now reboot the board typing 

        reboot
    
    
      


