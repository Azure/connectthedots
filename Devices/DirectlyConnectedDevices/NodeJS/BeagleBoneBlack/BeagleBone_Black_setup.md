This document explains how to set up a BeagleBone Black board to send data to Azure IoT services Hub using the REST interface. 
It assumes that you have the right tools installed and that you have cloned or downloaded the ConnectTheDots.io project on your machine.

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

* You will need the IP address of your BeaglBone, which  you can get typing the following command in the remote terminal:

        ifconfig

##Prepare settings files##
You will need to edit the settings file of the application before deploying the application on the board to apply your own connection information for your Azure Event Hub.

* Open the file connectthedots\Devices\DirectlyConnectedDevices\NodeJS\BeagleBoneBlack\settings.json and edit the settings based on the configuration of your ehdevices Event Hub and create a guid (i.e. using guidgen.com) for the device.

                 "namespace": "{namespace}",
                 "keyname": "{key-name}",
                 "key": "{key}",
                 "eventhubname": "ehdevices",
                 "guid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
                 "displayname": "BeagleBoneBlack",
                 "organization": "My Org",
                 "location":  "My location"

##Deploy the app##

The deployment is done using the deploy.cmd file found in the scripts subfolder:

* Edit the deploy.cmd file and enter your BeagleBone's IP address as well as the location on your machine of the PuTTY tools. Note that if you have setup a password for the user "root" on your board, you will have to switch a couple lines in the script (search for comments)
* Start the deploy.cmd script in a cmd shell. This will copy the app files into the /root/node_app_slot folder on the device and will update the node packages required for the app to run.
* After the files are copied in the /root/node_app_slot, you can test the app by typing the following commands in the remote terminal:

        cd /root/node_app_slot
        node beagleboneblackctd.js

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
    
    
      


