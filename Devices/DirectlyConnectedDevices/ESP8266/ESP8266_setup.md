This document explains how to connect a ESP8266 Adafruit Huzzah device to ConnectTheDots

##Prerequisites ##

### Required Software

- Azure Subscription (this is the subscription you want the services to be deployed to if you have several Azure subscriptions)
- [Git](https://git-scm.com/downloads) - For cloning the current repository
- Arduino IDE, version 1.6.8. (Earlier versions will not work with the Azure IoT library)
- Sensor libraries from Adafruit: DHT Sensor library, Adafruit Unified Sensor
- Deploy an instance of ConnectTheDots (see related chapter below) 

### Required Hardware

- Adafruit Huzzah ESP8266 IoT kit
  - Huzzah ESP8266 board
  - DHT22 Sensor
  - breadboard
  - M/M jumper wires
  - 10k Ohm Resistor (brown, black, orange)
  - A microB USB cable
  - A desktop or laptop computer which can run **Arduino IDE 1.6.8**

## Deploy The ConnectTheDots solution to your Azure subscription
If you have not done so already, follow all the instructions in the "Setup Tasks" paragraph of the [getting started guide](../../../GettingStarted.md). 
Once the solution is deployed, you will need to create a unique id for your device in the IoT Hub device registry as instructed in the same paragraph of the getting started guide.

## Connect the DHT22 Sensor Module to your Device

- Using [this image](https://github.com/Azure/connectthedots/blob/master/Devices/DirectlyConnectedDevices/ESP8266/images/huzzah_connect_the_dots.png?raw=true) as a reference, connect your DHT22 and Adafruit Huzzah ESP8266 to the breadboard

***
**Note:** Column on the left corresponds to sensor and on the Right to board. On the image, the board is place between 10 and 30 and sensor between 1 and 9. Additionally, when counting the - pins, start from the right and count in, as these do not align with the numbers indicated on the board.
***

- Connect the board, sensor, and parts on the breadboard:

| Start                   | End                    | Connector     |
| ----------------------- | ---------------------- | ------------ |
| Huzzah RST (Pin 30i)    | Huzzah CHPD (Pin 15i)  | Huzzah ESP8266 |
| DHT22 (Pin 1J)          | DHT22 (Pin 4J)         | DHT22         |
| NULL (Pin 2I)           | Pin 1F                 | 10k Ohm Resistor  |

- For the pins, we will use the following wiring:

| Start                   | End                    | Cable Color   | Connected to |
| ----------------------- | ---------------------- | ------------ | ------------- |
| VDD (Pin 1G)            | Pin 29J             | Red cable    | DHT22 |
| DATA (Pin 2G)           | Pin 17B             | White cable  | DHT22 |
| GND (Pin 4G)            | Pin 9-              | Black cable  | DHT22 |
| GND (Pin 27J)           | Pin 25-             | Black cable  | Huzzah ESP8266 |
| Pin 22B                 | Pin 6A              | Red cable    | Red LED  |
| Pin 21B                 | Pin 3A              | Green cable    | Green LED  |


- For more information, see: [Adafruit DHT22 sensor setup](https://learn.adafruit.com/dht/connecting-to-a-dhtxx-sensor).

**At the end of your work, your Adafruit Huzzah ESP8266 should be connected with a working sensor.**

## Run the application on the ESP8266

### Add the Adafruit Huzzah ESP8266 to the Arduino IDE
You will need to install the Adafruit Huzzah ESP8266 board extension for the Arduino IDE:

- Follow the instructions [here](https://learn.adafruit.com/adafruit-huzzah-esp8266-breakout/using-arduino-ide). There you will see how to add a URL pointing to Adafruit's repository of board extensions, how to make the Adafruit Huzzah ESP8266 board selectable under the **Tools** menu, and how to get the Blink sketch to run.
  - **Note**: There are two versions of Huzzah board, one with microB USB connector and other with a USB cable connected directly to the board. Both works properly with Azure IoT.
  - Boards with microB connector don't have the GPIO0 button. So, in the 'Blink Test', ignore the steps to put the board in the bootload mode, and go directly to the step to upload the sketch via the IDE.
- After going through this, you should have a working sample with a blinking light on your board.
    - If you run into any connection issues, unplug the board, hold the reset button, and while still holding it, plug the board back in. This will flash to board to try again.

### Install Library Dependencies

Open the file Devices\DirectlyConnectedDevices\ESP8266\connect_the_dots\connect_the_dots.ino in the Arduino IDE.

For this project, we'll  need the below libraries. To install them, click on the `Sketch -> Include Library -> Manage Libraries`. Search for each library using the box in the upper-right to filter your search, click on the found library, and click the "Install" button. 

 - DHT Sensor Library
 - Adafruit Unified Sensor
 - AzureIoTHub
 - AzureIoTUtility
 - AzureIoTProtocol_MQTT
 - ArduinoJSON

### Modify the code

- In the connect_the_dots.ino file, look for the following lines of code:

```
static char ssid[] = "[Your WiFi network SSID or name]";
static char pass[] = "[Your WiFi network WPA password or WEP key]";
```

- Replace the placeholders with your WiFi name (SSID), WiFi password, and the device connection string you created at the beginning of this tutorial. Save with `Control-s`
- Open up the file `connect_the_dots.cpp`. Look for the following lines of code and replace the placeholders connection information (this is the Device information that you've created when adding a new device id in the IoT Hub device registry):

```
static const char* deviceId = "[deviceid]";
static const char* connectionString = "[connectionstring]";
```

- You can also change the location, organization and displayname values to the ones of your choice
- Save all changes

### Compile and deploy the sample

- Select the COM port on the Arduino IDE. Use **Tools -&gt; Port -&gt; COM** to select it.
- Use **Sketch -&gt;  Upload** on Arduino IDE to compile and upload to the device.

At this point your device should connect to Azure IoT Hub and start sending telemetry data to your ConnectTheDots solution.