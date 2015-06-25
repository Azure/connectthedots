## Particle+DHT11 Setup ##
This wiki will describe how to connect the DHT11 sensor to your Particle Core and upload the code to connect it to your ConnectTheDots Event Hub.

## Prerequisites ##
Ensure that you have completed  setting up the [Particle Webhook](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ParticleCore/ParticleWebHook/ParticleWebHook-Setup.md) and [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md)

## Hardware Requirements ##
[Particle Core Device](https://store.particle.io/?product=particle-core)

[3 Solderless Male to Female Jumper Wires](http://www.amazon.com/YOUCable-Multicolor-Waterproof-Solderless-Breadboard/dp/B00L51YKSG/)

[DHT11 Sensor](http://www.amazon.com/gp/product/B00AF22GDC/ref=oh_aui_detailpage_o00_s00?ie=UTF8&psc=1)

## Wiring Instructions ##
Using your solderless breadboard jumpers, connect GNG to GND, VCC to 3v3, and DAT to D2 on your Particle Core as shown below:

![Particle+DHT11 Wiring Instructions](Particle+DHT11.jpg)

## Code Deployment ##
Ensure your Particle Core is configured for deployment by following this [Instructable](http://www.instructables.com/id/Getting-a-Spark-Core-running-without-using-Sparks-/).

[Login to the Particle IDE](https://build.particle.io/build) and create a new project.

Add the ADAFRUIT_DHT library by following the [Flash apps with Particle Build Using Libraries Documentation](http://docs.particle.io/build/#flash-apps-with-particle-build-using-libraries).

Create a new app and paste in the following [code](https://github.com/MSOpenTech/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ParticleCore/Particle%2BDHT11/Particle%2BDHT11.ino).

Update the following variables:

    char Org[] = "ORGANIZATION_NAME";
    char Disp[] = "DISPLAY_NAME";
    char Locn[] = "LOCATION";

Verify and Deploy to your Particle Core!
