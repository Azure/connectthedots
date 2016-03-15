# Taming the Beast: An open approach for communicating with similar but incongruous devices

There are a huge number of products in the Internet of Things (IoT) space, focusing on addressing 
requirements for collecting, transporting, sharing, and analyzing 
streaming data. The elephant in the room, however, is that we have very little that addresses how this data can 
be understood by the applications to which it has been shared. How does an application know whether the number it
received from some device is a temperature reading in degrees Centigrade, or if the 00 at offset 1024 from the beginning of a record represents whether a light bulb is on
or off?
  
We are working on a community driven, open source approach to address this problem, working with industry and the open source community
on a way to create common schemas to describe "Things". We have created a new project on GitHub, [openT2T](https://github.com/openT2T), pushed some cross platform code to GitHub to show how this
would work in a few simple cases, and are looking for developers to weigh in and contribute to the project.  



## Before and after - the application developer experience

Suppose you want to build and sell a smart application for controlling the enviroment in peoples' homes (temperature, lighting, ambience, and so forth). Amongst the many
things you need to do, you need to be able to determine the current state of things (like the current occupancy and the temperature), and control a plethora of devices (like thermostats and light
switches). If you can mandate what devices a person has in their home, like specifying that the whole house is 
retrofitted with sensors and controls from one specific manufacturer, fine. But you can't. A homeoner may have
any of a zillion different devices and you'll need to write code to read and control every single one of them. Basically
a nightmare! Consider, for example, these three small code snippets just for reading the temperature from three different devices. 
If you are reading from an LM35 temperature sensor probe, you need code like this, adapted from
[here](http://duino4projects.com/arduino-temperature-sensor-code/):
```
tempC = analogRead(tempPin);
tempC = (5.0 * tempC * 100.0)/1024.0;
```
Or, if you are reading from a TMP36 temperature probe, you need code like this, adapted from [here](https://learn.adafruit.com/tmp36-temperature-sensor/using-a-temp-sensor):
```
float voltage = reading * 5.0;
float temperatureC = (voltage - 0.5) * 100 ;  
```
Finally, if you are reading directly from a 10K Ohm Thermistor on a custom breadboard, you need code like this, adapted from 
[here](http://computers.tutsplus.com/tutorials/how-to-read-temperatures-with-arduino--mac-53714):
``` 
RawADC = analogRead(tempPin)
Temp = log(((10240000/RawADC) - 10000));
Temp = 1 / (0.001129148 + (0.000234125 + (0.0000000876741 * Temp * Temp ))* Temp );
Temp = Temp - 273.15; 
```
All the code snippets have to embed specific knowledge of the characteristics of physical devices themselves, the 
problem being that each of the physical sensors outputs something that has to be interpreted differently. Does the application developer really need or want to know the difference between reading an LM35, a TMP36, or a thermistor? Ideally,
all the sensors output the same thing, 'Temperature' and the application developer just needs to worry about reading that
value and integrating it into their application. In our view, the task of turning the voltage level or analog/digital output 
of the bare metal into that one thing, 'Temperature', is the job of a "Translator" - a small piece of code that takes the
output of an actual thing and translates it into a common schema for application developers to access. It 
enables IoT applications to be loosely-coupled to the hardware rather than written in silos with the inevitable
segmentation. 


## How it works

The application is written in Node.js, and the readme.md file in the sample contains all the information you need to
modify, build, deploy, and run the application. The basic logic is as follows.

  * Industry organizations, companies, or individuals create **category schemas** to describe devices (e.g. a schema to 
  describe a light bulb or a thermometer). There are numerous existing initiatives to do this (such as 
  the [oneIoTa](https://www.iab.org/wp-content/IAB-uploads/2016/03/OCF-oneIoTa-Overview-Paper_v3.pdf) tool 
  from the [Open Connectivity Foundation](http://openconnectivity.org/), 
  [M2M](http://technical.openmobilealliance.org/Technical/technical-information/omna/lightweight-m2m-lwm2m-object-registry) 
  from the [Open Mobile Alliance](http://technical.openmobilealliance.org/Technical/), 
  the [Open Distributed Object Framework (openDOF)](https://opendof.org/), 
  [Project Haystack](http://project-haystack.org/), and [IOTDB: The Internet of Things Database](https://iotdb.org/).  
  * When a device developer wants to enable their device to participate in the IoT ecosystem, they develop
  some code we are calling a **translator** in Javascript, from that specific device type to a category 
  schema of their choice.
  * The IoT solution developer implements an IoT infrastructure (e.g. a field gateway to receive data from a device and
  transmit it to Microsoft's Azure cloud, and a web site to display the data) 
  An application developer wanting to display the data (e.g. on the Azure website), or control it (e.g. using
  Microsoft's IoT Suite device management) references the category schema data types sent by the device. 
    
For example, suppose you want to write an application to read the temperature from a [Texas Instruments CC2650 Sensor
Tag](http://www.ti.com/ww/en/wireless_connectivity/sensortag2015/?INTC=SensorTag). Here are the steps to do this, from the OpenT2T project:

  * Identify a target schema, for example:```[Taqi: Need something here]```
  * Write a translator for the data that comes off the CC2650 to that schema. The data that comes off
  the CC2650 is described by Texas Instruments in a spec sheet, available [here](http://www.ti.com/product/tmp006). You can see an example of that
  translator on the OpenT2T project [here](https://github.com/openT2T/translators/blob/master/org.openT2T.sample.superPopular.temperatureSensor/Texas%20Instruments%20Inc.%20CC2650%20SensorTag/js/thingTranslator.js).
  ```[Taqi: Need something here]```  
  * Write an application that consumes that data. You can see an example application on the OpenT2T project [here](https://github.com/openT2T/sampleapps). ```[Taqi: Need something here]```
  
The translator can be written in the language of your choice, though the goal is for it to be fully cross-platform.
The initial examples we published on GitHub are written in JavaScript using the node.js platform, but a developer
could use C++, C#, or Java if preferred.


## Where we are right now

[Status of code, what works, what doesn't. Status of involvement by third parties]


## Roadmap

[What we hope to see happening, what we plan to do as MS]


## How to get involved

[Call to action]
