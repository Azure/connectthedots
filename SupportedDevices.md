# Supported Devices #

Below you'll find a list of supported devices, which can be found under each parent directory.

### Connect The Dots Getting Started With RPi and Arduino project ###
If you are going to deploy the getting started project, you need to procure an Arduino UNO and Raspberry Pi, as shown in the documentation for those devices in the appropriate folders:

- [Arduino UNO R3 and weather shield](Devices/GatewayConnectedDevices/Arduino%20UNO/Weather/WeatherShieldJson/Hardware.md)
- [Raspberry Pi](Devices/Gateways/GatewayService/Hardware.md)

Once you have these, head over to the [Getting Started With RPi and Arduino project](GettingStarted.md) to get going.

## Additional devices ##
If you decide to connect another device, you can check out the samples provided in the devices sub folder containing .NET, C++ and Node.js examples. Other languages examples are coming soon! Additionally, we encourage the community to submit new devices.  See the [Contribute](Contribute.md) page for details on how to do that.

The devices currently showcased are the following:

- [Directly connected devices](Devices/DirectlyConnectedDevices/):
    - Intel Edison running a node.js application sending data from a TI SensorTag BLE sensor kit
    - Intel Edison running a node.js application sending data from a Grove sensor kit
    - Intel Edison running a node.js application sending data from a Xadow sensor kit
    - node.js application sending data from a TI SensorTag BLE sensor kit
    - BeagleBone Black running a node.js application sending data from Grove sensors
    - Windows 10 Universal Application (for Windows 10, Windows 10 IoT Core, Windows 10 Mobile) sending simulated data
    - Windows 10 Universal Application (for Windows 10, Windows 10 Mobile) sending sensors data from a Microsoft Band
    - Xamarin application sending simulated data
    - ESP8266 powered board (such as Adafruit Feather Huzzah) running a C application.
- [Gateways](Devices/Gateways/GatewayService/):
    - Raspberry Pi supporting several types of device connections (see below) and running a C# service on top of Mono, or .NET Framework on Windows. 
- [Gateway connected devices](Devices/GatewayConnectedDevices/) (devices connecting to a gateway to send their data)
    - Arduino UNO with one or several of the following sensors
        - Accelerometer Memsic2125
        - Temperature sensor DS18B20
        - Simple sound sensor
        - Sparkfun weather shield
    - Arduino DUE with one or several of the following sensors:
        - Temperature sensor DS18B20
    - Wensn Sound Level Meter connected to the Gateway over USB
    - and more...

For all the above mentioned devices setup instructions, see [Device setup](Devices/DeviceSetup.md).