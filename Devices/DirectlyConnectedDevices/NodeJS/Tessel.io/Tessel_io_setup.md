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

Open the file connectthedots\Devices\DirectlyConnectedDevices\NodeJS\Tessel.io\connectthedotstessel.js and edit the settings based on the configuration of your ehdevices Event Hub
    
    namespace: '[namespace]',
    keyname: '[keyname]',
    key: '[key]',
    eventhubname: 'ehdevices',
    guid: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
    displayname: 'Tessel',
    organization: 'MS Open Tech',
    location:  'Here'

##Run the app##

Start the app typing the following command in a command prompt from the project's folder
    
    tessel run connectthedotstessel.js


