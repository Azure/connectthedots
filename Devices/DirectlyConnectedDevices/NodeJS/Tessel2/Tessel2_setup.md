This document explains how to set up a Tessel 2 board to send data to Azure IoT services Hub using the AMQP interface.

## Hardware requirements ##
Check out the hardware requirements [here](hardware.md).

## Prerequisites ##

To deploy the application you will need the following:

* Your preferred command line terminal
* Wifi internet access for the Tessel 2 and your computer

To work on the code of the project, you can use your favorite code editor.

## Configure the Tessel 2 ##

* Connect the Ambient module to Port A of the Tessel 2
* Connect to the Tessel 2 via USB from your computer
* Follow the instructions on the [Tessel 2 site](http://tessel.github.io/t2-start/) to install the t2 CLI tool, and connect the Tessel 2 to WiFi


## Set up the app on the board ##

* In your preferred terminal, type the following commands:

                 mkdir t2_connect_the_dots
                 cd t2_connect_the_dots
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/Tessel2/tessel2ctd.js
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/Tessel2/package.json
                 wget https://github.com/Azure/connectthedots/raw/IoTHub/Devices/DirectlyConnectedDevices/NodeJS/Tessel2/settings.json
                 npm install

* Before running the app, you need to update the settings.json file to input the device's connection string and a unique device id.
Following the instructions [here](../../../readme.md), to get the connection string for your device.

* In your preferred code editor, open the file settings.json and edit the settings based on the configuration of your ehdevices Event Hub. Note that the device id shall be unique per device so that data is not messed up in the connectthedots portal.

                 "iothubconnectionstring": "<connectionstring>",
                 "deviceid": "<deviceid>",
                 "displayname": "My Tessel 2",
                 "organization": "My Org",
                 "location":  "My location"

* Once you have changed the settings file, you can test your app. To do this, you'll be using the t2 CLI to transfer your code over to the Tessel 2 while it's connected (you need to be in the /t2_connect_the_dots folder):

        t2 run tessel2ctd.js

* In order to run your app on the Tessel without tethering, use the `push` command instead:

        t2 push tessel2ctd.js
