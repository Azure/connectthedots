## Spark Core Setup ##
This Wiki explains how to setup a [Spark Core](https://store.spark.io/?product=spark-core) to send temperature and optional humidity data to Microsoft Azure for analytics, real time data display, and alerts. It assumes that you have all the necessary hardware and preconditions satisfied (see below)

## Hardware Requirements ##
[Spark Core Device]()

AND

[DHT11 Sensor](http://www.amazon.com/gp/product/B00AF22GDC/ref=oh_aui_detailpage_o00_s00?ie=UTF8&psc=1)

OR

([DS18B20 Sensor with 4.7k Resistor](http://www.adafruit.com/products/381) And [Spark Button](https://www.spark.io/button))

## 

## Service Requirements
[Azure Subscription](http://azure.com)

[Spark.io account](http://spark.io)

## Preconditions
You will need an [AzureMobileServiceEventHub Proxy](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/SparkCore/AzureMobileServiceEventHubProxy-Setup.md) configured to relay data to your existing [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md)