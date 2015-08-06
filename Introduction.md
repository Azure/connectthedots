# Introduction #
## Overview ##
ConnectTheDots is put together to demonstrate the power of Azure IoT and its use of data from various devices.  It's built off the assumption that the sensors get the raw data and format it into a JSON string.  That string is then shuttled off to the Azure Event Hub, where it gathers the data and displays it as a chart.  Optional other functions of the Azure cloud sending alerts and averages, however this is not required.

The JSON string is sent to the Event Hub one of two ways: packaged into an AMQP message or in a REST packet.  This can be done via a Gateway, which is how the [Getting Started](Gettingstarted.md) sample does it, or through a device that is directly connected to the Event Hub, if the device is capable.  More details on each of those options are below.

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

To send data from a device to a gateway, you can just use the same exact data format and a device protocol adapter to implement any transport of your choice. The device protocol adapter is an assembly that implements the DeviceAdapterAbstract type to collect data from the device and enqueu them to the gateway for upload to the cloud. The Gateway automatically loads the device adapters from the Gateway binary directory, so deployement is extremely simple. 
You can find some examples under [Devices/Gateways/GatewayService/DeviceAdapters](Devices/Gateways/GatewayService/DeviceAdapters), and the matching devices under  [Devices/GatewayConnectedDevices](Devices/GatewayConnectedDevices). 

We even have some devices running in separate processes as a Python script, sending data to an adapter Gateway on a socket or a serial port connection. It does not get any easier than that!

## Software prerequisites ##
In order to reproduce one of the ConnectTheDots.io scenarios, you will need the following:

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. Visual Studio 2013 – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) or above
1. [WiX Toolset](http://wixtoolset.org) - if you want to build installer of Gateway for Windows

## Hardware prerequisites for Connect The Dots starter solution ##
If you are going to deploy the starter solution, you need to procure an Arduino UNO and Raspberry Pi, as shown in the documentation for those devices in the appropriate folders:

- [Arduino UNO R3 and weather shield](Devices/GatewayConnectedDevices/Arduino UNO/Weather/WeatherShieldJson/Hardware.md)
- [Raspberry Pi](Devices/Gateways/GatewayService/Hardware.md)

If you decide to connect another device, you can check out the samples provided in the devices sub folder containing .NET, C++ and Node.js examples. Other languages examples are coming soon! The devices currently showcased are the following:

- [Directly connected devices](Devices/DirectlyConnectedDevices/):
    - Intel Galileo running a C++ application and sending data from an Arduino compatible Weather Shield over AMQP
    - Intel Edison running a node.js application sending data from a TI SensorTag BLEn sensor kit over HTTP/REST
    - Gadgeteer device running a C# .Net Micro Framework application sending Gadgeteer sensors data over AMQP
    - Raspberry Pi 2 running Windows 10 IoT Core and a Universal Application sending dummy data over HTTP/REST
    - Windows Phone C# application sending the phone sensors (light and accelerometer) over HTTP/REST
    - Windows Phone C# application sending a data from a paired Microsoft Band (accelerometer, body temperature, heartbeat)over HTTP/REST
- [Gateways](Devices/Gateways/GatewayService/):
    - Raspberry Pi supporting several types of device connections (see below) and running a C# service on top of Mono, or .NET Framework on Windows, sending data over AMQP. 
- [Gateway connected devices](Devices/GatewayConnectedDevices/) (devices connecting to a gateway to send their data)
    - Arduino UNO with one or several of the following sensors
        - Accelerometer Memsic2125
        - Temperature sensor DS18B20
        - Simple sound sensor
        - Sparkfun weather shield
    - Arduino DUE with one or several of the following sensors:
        - Temperature sensor DS18B20
    - Wensn Sound Level Meter connected to the Gateway over USB

For all the above mentioned devices setup instructions, see the next section below, Step 3 (Device Setup).

