## Spark+DHT11 Setup ##
This wiki will describe how to connect the DS18B20 sensor to your Spark Core and upload the code to connect it to your ConnectTheDots Event Hub.  Using the Spark Button, we will give color feedback on the temperature from the device itself, in addition to publishing the temperature data to a ConnectTheDots Event Hub.

## Prerequisites ##
Ensure that you have completed  setting up the [AzureMobileServiceEventHub Proxy](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore/AzureMobileServiceEventHubProxy/AzureMobileServiceEventHubProxy-Setup.md) and [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md)

## Hardware Requirements ##
[Spark Core Device](https://store.spark.io/?product=spark-core)

[DS18B20 Sensor with 4.7k Resistor](http://www.adafruit.com/products/381)

[Spark Button](https://www.spark.io/button)

Soldering Iron / Solder

## Wiring Instructions ##

With the DS18B20 sensor in hand, solder VDD (Red Wire) to 3v3, GND (Black Wire) to GND, and D (White Wire) to D7 on the Spark Button board.  You will then want to connect a 4.7k Ohm resistor between VDD and D.  It is recommended that you perform the soldering while the Spark Core is detached from the Spark Button board.  For more details on this circuit, see [ContractorWolf's - The Spark Core IOT Temperature Sensor with the DS18b20](http://contractorwolf.com/sparkcore-temp-ds18b20/).  The finished product should resemble the images below:

![Spark+DHT11 Wiring Instructions](Spark+DS18B20-1.jpg)

![Spark+DHT11 Wiring Instructions](Spark+DS18B20-2.jpg)

## Code Deployment ##
Ensure your Spark Core is configured for deployment by following this [Instructable](http://www.instructables.com/id/Getting-a-Spark-Core-running-without-using-Sparks-/).

[Login to the Spark IDE](https://build.spark.io/build) and create a new project.

Add the SPARKBUTTON, HTTPCLIENT, and SPARKTIME libraries by following the [Flash apps with Spark Build Using Libraries Documentation](http://docs.spark.io/build/#flash-apps-with-spark-build-using-libraries).

Manually create 2 custom libraries by clicking the "+" to the far right of the Spark Web IDE.  Name  the first "DS18B20" and the second "OneWire".  This will create a DS18B20.cpp, DS18B20.h, OneWire.cpp, and OneWire.h file within the Spark IDE. Once these have been created, copy and paste in the contents for each respective file from the [Spark+DS18B20 Repo](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore/Spark+DS18B20).

Within the .ino of your newly created project, paste in the following [code](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore/Spark+DS18B20/Spark+DS18B20.ino).

Verify and Deploy to your Spark Core!