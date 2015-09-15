This document explains how to set up a Tessel.io board to send data to Azure Event Hub. 
It assumes that you have the right tools installed and that you have cloned or downloaded the ConnectTheDots.io project on your machine.

##Hardware requirements ##

Check out the hardware requirements [here](hardware.md).
Connect the Ambient module to connector A
Connect the Climate module to connector B

## Configure the Tessel##

Follow the instructions on the [Tessel site](http://start.tessel.io/install) to setup your environment and the Tessel board.
Make sure you have setup the Wifi connection so that your Tessel board can send data to Azure.

##Prepare settings files##
You will need to edit the settings file of the application before deploying the application on the board to apply your own connection information for your Azure Event Hub.

Open the file connectthedots\Devices\DirectlyConnectedDevices\NodeJS\Tessel.io\tesselctd.js and edit the settings based on the configuration of your ehdevices Event Hub
Also create a new guid for the device (i.e. using a tool such as guidgen.com)
    
    namespace: '[namespace]',
    keyname: '[keyname]',
    key: '[key]',
    eventhubname: 'ehdevices',
    guid: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
    displayname: 'Tessel',
    organization: 'My Org',
    location:  'My Location'
    
In the same file, edit the settings for Wifi connection, enter the SSID and key for your access point

    ssid: '[SSID]',
    password: '[Key]',
    security: 'wpa2',
    timeout: 30
    
IMPORTANT: In the same file you will have to add an SAS Token as the Tessel doesn't support SH256 encryption which is required to generate an SAS Token used to secure connection with Azure IoT services
Search for the call to the function connectthedots.init_connection in the code and enter an SAS Token generated using a tool such as the one mentionned in the comment above the code.
    
##Run the app##

In the a command prompt, navigate to the project's folder connectthedots\Devices\DirectlyConnectedDevices\NodeJS\Tessel.io and run the following commands to initialize and install dependencies

    npm install 

Start the app typing the following command in a command prompt from the project's folder
    
    tessel run tesselctd.js


