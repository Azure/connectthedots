RedBear BLE Nano with Decagon 10HS Moisture Sensor 
===================================================

Hardware Configuration
----------------------

1.  10HS sensor input connected to P0\_4

2.  10HS power tied to P0\_9, P0\_10 and P0\_11 (this will be switched on for 20ms every 5 seconds to extend battery life)

3.  3xAA lithium cells connected to VIN. Batteries should last 4+ years with continuous use.

Code Compilation
----------------

1.  Create a project at <https://developer.mbed.org/>

2.  Import the code in main.cpp

3.  Import the BLE\_API and nRF51822 libraries

4.  Ensure device target is set to BLE nano

5.  Compile and download the generated .hex file to the BLE nano device

Device Usage
------------

Since the data is sent over the advertising packet, no device paring is required. Simply turn the device on.
