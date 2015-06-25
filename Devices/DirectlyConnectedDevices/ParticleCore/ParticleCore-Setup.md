## Particle Core Setup ##
This Wiki explains how to setup a [Particle Core](https://store.particle.io/?product=particle-core) to send temperature and optional humidity data to Microsoft Azure for analytics, real time data display, and alerts. It assumes that you have all the necessary hardware and preconditions satisfied (see below)

## Hardware Requirements ##
[Particle Core Device](https://store.particle.io/?product=particle-core)

AND

[DHT11 Sensor](http://www.amazon.com/gp/product/B00AF22GDC/ref=oh_aui_detailpage_o00_s00?ie=UTF8&psc=1)

OR

([DS18B20 Sensor with 4.7k Resistor](http://www.adafruit.com/products/381) And [Particle Button](https://www.particle.io/button))

## 

## Service Requirements
[Azure Subscription](http://azure.com)

[Particle.io account](http://particle.io)

## Preconditions
You will need an [Particle Webhook](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ParticleCore/ParticleWebHook/ParticleWebHook-Setup.md) configured to relay data to your existing [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md).

## Projects ##
Once you have satisfied the above chose the appropriate project based on your hardware:

[Particle + DHT11](https://github.com/toolboc/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ParticleCore/Particle%2BDHT11/Particle%2BDHT11-Setup.md)

[Particle + DS18B20 w / Particle Button](https://github.com/toolboc/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ParticleCore/Particle%2BDS18B20/Particle%2BDS18B20-Setup.md)