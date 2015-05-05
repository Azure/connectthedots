## Spark+DHT11 Setup ##
This wiki will describe how to connect the DHT11 sensor to your Spark Core and upload the code to connect it to your ConnectTheDots Event Hub and .

## Prerequisites ##
Ensure that you have completed  setting up the [AzureMobileServiceEventHub Proxy](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore/AzureMobileServiceEventHubProxy/AzureMobileServiceEventHubProxy-Setup.md) and [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md)

## Hardware Requirements ##
[Spark Core Device](https://store.spark.io/?product=spark-core)

[3 Solderless Breadboard Jumper Wires](http://www.amazon.com/Wosang-Solderless-Flexible-Breadboard-Jumper/dp/B005TZJ0AM/ref=sr_1_1?ie=UTF8&qid=1430780052&sr=8-1&keywords=solderless+jumper+wires)

[DHT11 Sensor](http://www.amazon.com/gp/product/B00AF22GDC/ref=oh_aui_detailpage_o00_s00?ie=UTF8&psc=1)

## Wiring Instructions ##
Using your solderless breadboard jumpers, connect GNG to GND, VCC to 3v3, and DAT to D2 on your Spark Core as shown below:

![Spark+DHT11 Wiring Instructions](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/Spark+DHT11.jpg)

## Code Deployment ##
Ensure your Spark Core is configured for deployment by following this [Instructable](http://www.instructables.com/id/Getting-a-Spark-Core-running-without-using-Sparks-/).

[Login to the Spark IDE](https://build.spark.io/build) and create a new project.

Add the ADAFRUIT_DHT, HTTPCLIENT, and SPARKTIME libraries by following the [Flash apps with Spark Build Using Libraries Documentation](http://docs.spark.io/build/#flash-apps-with-spark-build-using-libraries).

Create a new app and paste in the following [code](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore\Spark+DHT11\Spark+DHT11.ino).

Verify and Deploy to your Spark Core!