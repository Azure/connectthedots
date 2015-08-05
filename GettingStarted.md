#Getting Started#
As good first task we'll walk you through building a simple temperature sensing network. It can be built quickly and easily with minimal knowledge of programming or Microsoft Azure, using commodity devices available locally or online - for example an Arduino UNO board with a weather shield, connected to a Raspberry Pi sending data to an Azure website. 


![](Arduino-Pi-IoT.png)


Sample code for this is included in the project, as well as for many other more elaborate scenarios.

## Hardware prerequisites for Connect The Dots starter solution ##
If you are going to deploy the starter solution, you need to procure an Arduino UNO and Raspberry Pi, as shown in the documentation for those devices in the appropriate folders:

- [Arduino UNO R3 and weather shield](Devices/GatewayConnectedDevices/Arduino UNO/Weather/WeatherShieldJson/Hardware.md)
- [Raspberry Pi](Devices/Gateways/GatewayService/Hardware.md)

If you decide to connect another device, you can check out the samples provided in the devices sub folder containing .NET, C++ and Node.js examples. Other languages examples are coming soon! The devices currently showcased are the following:

- Directly connected devices:
    - Intel Galileo running a C++ application and sending data from an Arduino compatible Weather Shield over AMQP
    - Intel Edison running a node.js application sending data from a TI SensorTag BLEn sensor kit over HTTP/REST
    - Gadgeteer device running a C# .Net Micro Framework application sending Gadgeteer sensors data over AMQP
    - Raspberry Pi 2 running Windows 10 IoT Core and a Universal Application sending dummy data over HTTP/REST
    - Windows Phone C# application sending the phone sensors (light and accelerometer) over HTTP/REST
    - Windows Phone C# application sending a data from a paired Microsoft Band (accelerometer, body temperature, heartbeat)over HTTP/REST
- Gateways:
    - Raspberry Pi supporting several types of device connections (see below) and running a C# service on top of Mono, or .NET Framework on Windows, sending data over AMQP. 
- Gateway connected devices (devices connecting to a gateway to send their data)
    - Arduino UNO with one or several of the following sensors
        - Accelerometer Memsic2125
        - Temperature sensor DS18B20
        - Simple sound sensor
        - Sparkfun weather shield
    - Arduino DUE with one or several of the following sensors:
        - Temperature sensor DS18B20
    - Wensn Sound Level Meter connected to the Gateway over USB

For all the above mentioned devices setup instructions, see the next section below, Step 3 (Device Setup).

## Setup Tasks ##
Setting up your IoT solution involves several distinct steps, each of which is fully described in this project:


1. Clone or copy the project to your machine (NOTE: place the project in a folder as close to the root of your file system as possible. Some paths in the project are very long and you might encounter issues with long path names when restoring NuGet packages)
1. [Azure prep](Azure/AzurePrep/AzurePrep.md) - Creating basic Azure resources
1. [Device setup](Devices/DeviceSetup.md) - Configuring your device(s)
1. [Sample website deployment](Azure/WebSite/WebsitePublish.md) - Publishing a generic sample website for viewing the data
2. [Stream Analytics integration](Azure/StreamAnalyticsQueries/SA_setup.md) - Configuring Stream Analytics to send alerts and averages to the sample website.
  
To get started with a simple example, complete the "Connect The Dots starter solution" tasks identified in each of the above steps.

## Run the scenario ##

Once you have setup the services, published the site, provisioned and connected your devices, you will see data coming up on your website at the URL you chose when deploying the site.

You should see average temperature measurements showing up in the web browser every 20 seconds.

If you select “All”, you should see raw readings from the device coming in every second.
If the temperature exceeds 75 degrees (F), you should see an alert showing in the alerts table, once every 20 seconds while the temperature on any of the devices exceeds 75 degrees (F).
If you cover the shield, you will see an alert telling you the light is turned off.

![](WebSiteCapture.png)
