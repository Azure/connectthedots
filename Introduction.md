# Introduction #
## Overview ##
ConnectTheDots is put together to demonstrate the power of Azure IoT and its use of data from various devices.  It's built off the assumption that the sensors get the raw data and format it into a JSON string.  That string is then shuttled off to the Azure Event Hub, where it gathers the data and displays it as a chart.  Optional other functions of the Azure cloud include sending alerts and averages, however this is not required.

The JSON string is sent to the Event Hub one of two ways: packaged into an AMQP message or in a REST packet.  This can be done via a Gateway, which is how the [Getting Started](Gettingstarted.md) sample does it, or through a device that is directly connected to the Event Hub, if the device is capable.  More details on each of those options are below.

We encourage the community to contribute to the project!  See [Contribute](Contribute.md) page for details.

## Device basics ##
The current project is built on the premise that data from sensors is sent to an Azure Event Hub in a prescribed JSON format. The minimum structure, with required attribute names, is 

    {
	"guid" 			:	"string",
	"organization"	:	"string",
	"displayname"	:	"string",
	"location"		:	"string",
	"measurename"	:	"string",
	"unitofmeasure"	:	"string",
	"value" 		:	double/float/integer
	}
	
This should all be sent as one string message to the Event Hub, for example as the following strings: 

    {"guid":"62X74059-A444-4797-8A7E-526C3EF9D64B","organization":"My Org Name","displayname":"Sensor Name","location":"Sensor Location","measurename":"Temperature","unitofmeasure":"F","value":74}

or

    {"guid":"62X74059-A444-4797-8A7E-526C3EF9D64B","organization":"my org name","displayname":"sensor name","location":"sensor location","measurename":"Temperature","unitofmeasure":"F","value":74.0001}


Furthermore, the project is built upon the premise that the *sensors* create and format this JSON string. For example, if using a sensor attached to an Arduino, the code running on the Arduino would send successive JSON strings, CRLF ended, out the serial port to a gateway such as a Raspberry Pi or Windows Tablet. The gateway does nothing other than receive the JSON string, package that into an AMQP message, and send it to Azure. In the case of a directly connected device, the latest needs to send the JSON package to the event hub whether encapsulating the JSON message in an AMQP message or sending the JSON message in a REST packet.

All the device code included in this project, or submitted for inclusion, must conform to the JSON format requirement above. 

### Devices and Gateway ###
ConnectTheDots provides a Gateway to collect data from devices that cannot, or should not, target the cloud directly. The Gateway code is tested in Mono for Linux and on the .NET Framework on Windows. It is located at in the source tree under [Devices/Gateways/GatewayService](Devices/Gateways/GatewayService/), and is a simple system service. 

To send data from a device to a gateway, you can just use the same exact data format and a device protocol adapter to implement any transport of your choice. The device protocol adapter is an assembly that implements the DeviceAdapterAbstract type to collect data from the device and enqueue them to the gateway for upload to the cloud. The Gateway automatically loads the device adapters from the Gateway binary directory, so deployment is extremely simple. 
You can find some examples under [Devices/Gateways/GatewayService/DeviceAdapters](Devices/Gateways/GatewayService/DeviceAdapters), and the matching devices under  [Devices/GatewayConnectedDevices](Devices/GatewayConnectedDevices). 

We even have some devices running in separate processes as a Python script, sending data to an adapter Gateway on a socket or a serial port connection. It does not get any easier than that!

## Software prerequisites ##
In order to reproduce one of the ConnectTheDots.io scenarios, you will need the following:

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. Visual Studio 2013 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient
1. [WiX Toolset](http://wixtoolset.org) - if you want to build installer of Gateway for Windows

## Where to start ##
If this is you're first time with the project, we suggest you head over to the [Getting Started project](GettingStarted.md) to learn the basics.  If you'd like to create your own solution with Azure IoT, check out the [supported devices](SupportedDevices.md).



