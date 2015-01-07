This Wiki explains how to setup an Arduino Uno R3 with a Sparkfun weather shield to be used in the Temperature and Humidity monitoring scenario of ConnectTheDots.io.
It assumes that you have the right tools installed and that you have cloned or downloaded the ConnectTheDots.io project on your machine.

##Hardware requirements

1. [Arduino Uno R3](http://arduino.cc/en/Main/ArduinoBoardUno)
1. [Sparkfun Weather Shield](https://www.sparkfun.com/products/12081) for Arduino 
1. 4 stackable headers for the Arduino UNO R3 (1 @ 10 pin, 2 @ 8 pin, 1 @ 6 pin), as specified on the Arduino or Sparkfun sites. 
1. USB A to B cable to connect the Arduino to the Raspberry Pi. Make sure it is USB 2.0 not 3.0, as the 3.0 connector does not fit the Arduino.

You will need to solder the headers to the Weather Shield (or have someone do that for you - 10 minute job, max.)

Note: Only the models above have been tested. The Weather Shield sample code from Sparkfun, for example, may need to be modified before it will work reliably on Arduino Due or Arduino Uno R2.

##Prepare the Arduino Uno R3:

* Connect the Arduino Uno R3 directly to your computer with the USB cable
* Install and run the Arduino IDE (we recommend the 1.5.8 version, with the Windows Installer) which you can find on the [Arduino.cc site](http://arduino.cc/en/Main/Software).
* If necessary, install the Windows device drivers for the Arduino on your computer, following the instructions [here](http://arduino.cc/en/Guide/Windows#toc4).
* Download the Weather Shield libraries from [here](https://dlnmh9ip6v2uc.cloudfront.net/assets/b/5/9/7/f/52cd8187ce395fa7158b456c.zip) (as per the instruction in the [Weather Shield Hookup guide](https://learn.sparkfun.com/tutorials/weather-shield-hookup-guide)).
    * Extract all files from the ZIP file to a temporary location, then import the two folders (HTU21D_Humidity and MPL3115A2_Pressure) in the Arduino IDE by clicking Sketch, Import Library, Add Library and selecting the folder (once for each folder).
* In the Arduino IDE open Devices\Arduino\WeathershieldJson.ino (it is modified from the original Spark fun sample to send data in JSON format). Edit the code to set your own name for the sensor (which will be displayed in the website):
```c
char DeviceDisplayName[] = "MySensorName"; 
```
* Compile and upload the Weather Shield sketch to the Arduino: File/Upload or Crtl-U or press the Right Arrow. 
* Open the serial monitor (shift-ctrl-m). You should now see temperature and other data on the serial monitor. Note the format of the data being sent out the serial port of the Arduino – in the next section you’ll see the same data being read from the serial port of the Raspberry Pi:
![](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/ArduinoCOMCapture.png)
* Disconnect the Arduino from your computer

##Connect the Arduino board to the gateway

* Plug Arduino’s USB cable into one of the Raspberry Pi USB ports:
![](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/PiAndArduinoPhoto.jpg)