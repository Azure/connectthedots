# Device setup  #
The basic premise of this project is that data from sensing devices can be sent upstream and received in a prescribed JSON format. This might be achieved by programming the devices themselves (e.g. compiling and uploading a Wiring script to an Arduino UNO), or by reading the data from the device and formatting it accordingly (e.g. using a Python script on a Raspberry Pi to read USB output from a commercial Sound Level Meter). 

## Creating devices IDs for Azure IoT Hub ##
The ConnecttheDots project uses Azure IoT Hub to connect devices to the Cloud.
When [deploying the full solution using the ARM template](../Azure/ARMTemplate/Readme.md) an Azure IoT Hub is deployed as part of your solution.

The ConnectTheDots website allows you to manage your IoT Hub (Create/Delete devices, get connection strings) directly from the site. But for security reasons, there is a configuration step described in the [service deployment instructions](../Azure/ARMTemplate/readme.md#enable-iothub-management).
If you have enabled this feature, then you can manage the devices directly from your ConnectTheDots website.

If you have not enabled the feature, then , you can find connection information for managing the IoT Hub instance in the [Azure portal](http://portal.azure.com). Search for the Resource Group with the name you used for the solution when deploying the services using the script.
For each of the devices that you want to connect to your ConnectTheDots solution, you will need to create a new device ID.
You will find all the instructions to create device IDs and retrieve connection strings [here](https://github.com/Azure/azure-iot-sdks/blob/master/doc/manage_iot_hub.md).

## ConnectTheDots getting started project using Raspberry Pi and Arduino ##
For this project, follow the instructions for configuring the following:

1. [Arduino UNO with weather shield](GatewayConnectedDevices/Arduino UNO/Weather/WeatherShieldJson/Arduino-and-Weather-Shield-setup.md) 
2. [Raspberry Pi](Gateways/GatewayService/RaspberryPi-Gateway-setup.md) 

## Connect The Dots with all the other devices ##

To build your own end-to-end configuration you need to identify and configure the device(s) that will be producing the data to be pushed to Azure and displayed/analyzed. Devices fall generally into two categories - those that can connect directly to the Internet, and those that need to connect to the Internet through some intermediate device or gateway. Sample code and documentation can be found in the following folders:

1. [Simple devices requiring a gateway](GatewayConnectedDevices/) - Devices too small or basic to support a secure IP connection, or which need to be aggregated before sending to Azure
2. [Devices connecting directly to Azure](DirectlyConnectedDevices/) - Devices powerful enough to support a secure IP connection
3. [Gateways or other intermediary devices](Gateways/) - Devices which collect data from other devices and upload to Azure. These can be very simple (e.g. just package and send the data securely to Azure without changes), or very sophisticated (e.g. allow for device authentication, provisioning, management, and communications). 


### Build a sensor infrastructure ###
For additional scenarios, or more advanced configurations, follow the setup instructions in the folders for the devices or gateways listed above.
