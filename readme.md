![](images/CTD-logo-v5-02.png)

[ConnectTheDots.io](http://connectthedots.io) is an open source project created by Microsoft to help you get tiny devices connected to Microsoft Azure IoT and to implement great IoT solutions taking advantage of Microsoft Azure advanced analytic services such as Azure Stream Analytics and Azure Machine Learning.

The project is built with the assumption that the sensors get the raw data and format it into a JSON string. That string is then sent to Azure IoT Hub, from which a Web app gathers the data and displays it as a chart.
Optional other functions of the Azure cloud include detecting and displaying alerts and averages, however this is not required.

The JSON string is sent to Azure IoT Hub whether directly by the sensor device if it is capable of connecting to Azure IoT Hub or through a multi-protocol Gateway, which is how the [Getting Started with Pi and Arduino](GettingStarted.md) sample does it. 
More details on each of those options are below.

![](images/ConnectTheDots-architecture.png)


In this project there are code samples, configuration scripts and guides that will help you set up devices and sensors, and configure Microsoft Azure services to view and analyze the data produced by those devices. Some of these samples have been provided by Microsoft, others by third parties; we encourage everyone to submit code samples or configuration documentation to grow this project.

This project contains several device samples all aimed at helping you connect your devices to Azure IoT, as well as visualize and gain insight from your data.  Check out all the samples below, or follow the getting started walkthrough to learn more. Then, add some of your devices to the project!

We encourage the community to contribute to the project! See [Contribute](Contribute.md) page for details.

## What's new in the V2? ##

Plenty!
The main change is the use of Azure IoT Hub instead of Event Hubs for connecting devices to the Cloud, but here is a list of what's new in V2

- Migrated from Event Hubs to Azure IoT Hub for the devices connectivity to Azure IoT: IoT Hub offers a better security with a per device authentication, along with a bidirectional messaging infrastructure.
- Replaced the Azure Prep tool with an ARM Template: you can now deploy the whole solution (including the WebSite) from a command line not only on Windows, but also no a Mac or a Linux machine!
- Updated devices samples
    - Updated all the node.js samples to use the Azure IoT Hub SDK
    - Removed old devices samples (Galileo, .Net Micro Framework)
    - Updated the Gateway code to use Azure IoT Hub device SDK
    - Upgraded WP8 samples (Simulated Sensors and MS Band) to Windows 10 UWP apps
    - Added Xamarin samples 
    - Added a sample for ESP8266 chips 

## Where is the V1 if I still want to use the old fashion way (using Event Hub)? ##
The V1 has been tagged and you can find the release as a binary [here](https://github.com/Azure/connectthedots/releases/tag/1.0)
We also created a [branch](https://github.com/Azure/connectthedots/tree/V1) that we will not make additions to any more but will definitively track to merge your contributions.

## Device basics ##

### Data format ###
ConnectTheDots is built on the assumption that data from sensors is sent to Azure IoT Hub in a prescribed JSON format. The minimum structure, with required attribute names, is 

```
{
    "guid":	"string",
    "organization":	"string",
    "displayname": "string",
    "location": "string",
    "measurename": "string",
    "unitofmeasure": "string",
    "timecreated": "string",
    "value": double/float/integer
}
```
	
This should all be sent as one string message to IoT Hub, for example as the following strings: 

    {"guid":"62X74059-A444-4797-8A7E-526C3EF9D64B","organization":"My Org Name","displayname":"Sensor Name","location":"Sensor Location","measurename":"Temperature","unitofmeasure":"F","timecreated":"1975-09-16T12:00:00Z", "value":74}

or

    {"guid":"62X74059-A444-4797-8A7E-526C3EF9D64B","organization":"my org name","displayname":"sensor name","location":"sensor location","measurename":"Temperature","unitofmeasure":"F","timecreated":"1975-09-16T12:00:00Z", "value":74.0001}


Furthermore, the project is built upon the assumption that the *sensors* create and format this JSON string.
For example, if using a sensor attached to an Arduino, the code running on the Arduino would send successive JSON strings, CRLF ended, out the serial port to a gateway such as a Raspberry Pi or Windows Tablet. The gateway does nothing other than receive the JSON string, package that into the right message format, adds the timecreated time stamp, and send it to Azure.

In the case of a directly connected device, the latest needs to send the JSON package to the IoT Hub leveraging one of the existing Azure IoT Hub device client SDKs.

All the device code included in this project, or submitted for inclusion, must conform to the JSON format requirement above. 

### Devices and Gateway ###
ConnectTheDots provides a Multi-protocol Gateway to collect data from devices that cannot, or should not, target the cloud directly. The Gateway code is tested on Mono for Linux and on the .NET Framework on Windows and is located in the source tree under [Devices/Gateways/GatewayService](Devices/Gateways/GatewayService/), and is a simple system service. 

To send data from a device to a gateway, you can just use the same exact data format and a device protocol adapter to implement any transport of your choice. The device protocol adapter is an assembly that implements the DeviceAdapterAbstract type to collect data from the device and enqueue them to the gateway for upload to the cloud. The Gateway automatically loads the device adapters from the Gateway binary directory, so deployment is extremely simple. 
You can find some examples under [Devices/Gateways/GatewayService/DeviceAdapters](Devices/Gateways/GatewayService/DeviceAdapters), and the matching devices under  [Devices/GatewayConnectedDevices](Devices/GatewayConnectedDevices). 

We even have some devices running in separate processes as a Python script, sending data to an adapter Gateway on a socket or a serial port connection. It does not get any easier than that!

## Software prerequisites ##
In order to reproduce one of the ConnectTheDots.io scenarios, you will need the following:

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. [optional] Visual Studio 2013 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient. Note that if you are not planning to use the Gateway nor making changes to the dashboard, you will NOT need Visual Studio.
1. [optional] [WiX Toolset](http://wixtoolset.org) - if you want to build installer of Gateway for Windows

## Getting Started ##

To get started with ConnectTheDots, you will beed to go through the following basic steps:

1. [Deploying services](Azure/ARMTemplate/Readme.md): this is easily done using the automated deployment script.
2. [Setup devices](Devices/DeviceSetup.md): follow these instructions to provision devices in the IoT Hub and modify and deploy code sample

## Available Devices ##
For a full list of devices and code samples check out the ever growing list of [supported gateways and sensors](SupportedDevices.md).
