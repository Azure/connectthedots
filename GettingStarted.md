# Getting Started #
As good first task we'll walk you through building a simple temperature sensing network. It can be built quickly and easily with minimal knowledge of programming or Microsoft Azure, using commodity devices available locally or online - for example an Arduino UNO board with a weather shield, connected to a Raspberry Pi sending data to an Azure website. 


![](images/Arduino-Pi-IoT.jpg)


Sample code for this is included in the project.

## Hardware prerequisites for Connect The Dots getting started project ##
If you are going to deploy the starter solution, you need to procure an Arduino UNO and Raspberry Pi, as shown in the documentation for those devices in the appropriate folders:

- [Arduino UNO R3 and weather shield](Devices/GatewayConnectedDevices/Arduino UNO/Weather/WeatherShieldJson/Hardware.md)
- [Raspberry Pi](Devices/Gateways/GatewayService/Hardware.md)

## Software prerequisites ##
In order to reproduce one of the ConnectTheDots.io scenarios, you will need the following:

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. Visual Studio 2013 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient
1. [WiX Toolset](http://wixtoolset.org) - if you want to build installer of Gateway for Windows

## Setup Tasks ##
Setting up your IoT solution involves several distinct steps, each of which is fully described in this project:


1. Clone or copy the project to your machine (NOTE: place the project in a folder as close to the root of your file system as possible. Some paths in the project are very long and you might encounter issues with long path names when restoring NuGet packages)
1. [Azure prep](Azure/AzurePrep/AzurePrep.md) - Creating basic Azure resources
1. [Device setup](Devices/DeviceSetup.md) - Configuring your device(s)
1. [Sample website deployment](Azure/WebSite/WebsitePublish.md) - Publishing a generic sample website for viewing the data
2. [Stream Analytics integration](Azure/StreamAnalyticsQueries/SA_setup.md) - Configuring Stream Analytics to send alerts and averages to the sample website.
  
To get started with our simple example, complete the tasks above in order. Navigation is provided on each page to get to the next topic.

## Run the scenario ##

Once you have setup the services, published the site, provisioned and connected your devices, you will see data coming up on your website at the URL you chose when deploying the site.

You should see average temperature measurements showing up in the web browser every 20 seconds.

If you select “All”, you should see raw readings from the device coming in every second.
If the temperature exceeds 80 degrees (F), you should see an alert showing in the alerts table, once every 20 seconds while the temperature on any of the devices exceeds 80 degrees (F).
If you cover the shield, you will see an alert telling you the light is turned off.

![](images/WebsiteCapture.jpg)
