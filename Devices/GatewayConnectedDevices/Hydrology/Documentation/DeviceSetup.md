Hardware Description
====================

This project uses 3 hydrology sensors:

-   A Dissolved Oxygen Sensor Board (<https://www.atlas-scientific.com/_files/_datasheets/_circuit/DO_Circuit_5.0.pdf>) with an appropriate probe (<https://www.atlas-scientific.com/product_pages/probes/do_probe.html>)

-   An Electrical Conductivity Sensor Board (<https://www.atlas-scientific.com/_files/_datasheets/_circuit/EC_EZO_Datasheet.pdf>) with an appropriate probe (<https://www.atlas-scientific.com/product_pages/probes/ec_k1-0.html>)

-   A Soil Moisture Sensor (<http://manuals.decagon.com/Manuals/13508_10HS_Web.pdf>) with an Analog-To-Digital converter (<https://www.adafruit.com/products/1085>)

The Raspberry PI 2 was chosen to both host the Device Gateway and support communication with the sensors.

The Dissolved Oxygen sensor board available only supported UART communication. Thus it was connected to the UART of the PI.

The soil moisture sensor has an analog output that needed to be converted to digital for the host computer. In order to do this an ADS1115 breakout board from Adafruit was added. This board uses the I2C bus to connect to the host system.

The Electrical Conductivity board supports both UART and I2C communication. I2C was chosen in this implementation in order to avoid having to add a UART multiplexer to the circuit configuration.

The schematic and image of the breadboard may be found in the Hydrology\\Documentation folder.

Raspberry PI Configuration
==========================

The I2C port needs to be enabled on the PI, and the login shell over serial needs to be disabled. This can be done with:

	sudo raspi-config

Choose AdvancedI2C Configuration enable, then choose AdvancesSerial Configurationdisable

Python Setup
============

You will need several packages installed for the python scripts to run:

    apt-get install I2C\_Tools python-serial python-smbbus

Running the Code
================

Individual sensors can be tested out by running the python script for that sensor (ECSensor.py, DO2Sensor.py, or MoistureSensor.py). Output will be sent to stdout.

SensorAgent.py gathers the data from each of the sensors and make it available via local host socket for consumption by the gateway service.

The deploy\_next.cmd script in the Hydrology folder can be used to deploy these scripts to the device after the Gateway service has been deployed. This will also install the SensorAgent.py script so that it runs at device startup.
