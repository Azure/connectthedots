This document explains how to run a simple node sample on a desktop machine (Windows, Linux, OSX)

##Prerequisites ##

To run the application you will need to have [node.js](http://nodejs.org) installed on your system.
The machine will also need to be connected to the internet

To work on the code of the project, you can use your favorite code editor. 

##Setup the app on the desktop##

* Once you have cloned or downloaded the repository, open a command prompt, and navigate to the application folder (Devices\DirectlyConnectedDevices\NodeJS\Desktop) and type the following command:

                 npm install
                 
* Before running the app, you need to update the settings.json file to input the device's connection string and a unique device id.
Following the instructions [here](../../../readme.md), get the connection string for your device.
                 
* Open the file settings.json in your favorite text editor and edit the settings using the device id and connection string generated following the previous instructions.

                 "iothubconnectionstring": "<connectionstring>",
                 "deviceid": "<deviceid>",
                 "displayname": "BeagleBoneBlack",
                 "organization": "My Org",
                 "location":  "My location"

##Run the app##
* To run the app, type the folowing command in the application folder:

        node .


