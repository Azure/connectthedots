This Wiki explains how to setup an Intel Galileo Gen1 or Gen2 board running Windows for IoT to send temperature and humidity data to Microsoft Azure for analytics, real time data display, and alerts.
It assumes that you have all the necessary hardware and tools installed (see below)

##Hardware requirements

 - [Intel Galileo Gen 1 Board][1]
   
	OR

 - [Intel Galileo Gen 2 Board][2]

**IMPORTANT NOTE**:

Intel Galileo Gen1 and Gen2 use different power supplies. Please only use the 12 volt power supply with the Gen2 and a 5 volt power supply with Gen1. 

##Supported Sensors

- [Sparkfun Weathershield][3]
- [Seeed Grove Starter Kit][4]
- [DHT11 Analog Temperature & Humidity Sensor][5]


##Software and tools requirements

You will need to install the below software in this order to implement this Intel Galileo sample.

 - Visual Studio 2013 ([Community Edition][5] works fine)
 
##Getting Started

* Register for the [Windows for IoT Developer Program][6]
* [Follow the instructions for setting up the Windows Developer Program for IoT Development environment][7]
* [Follow the instructions to install the Windows Developer Program for IoT on your Intel Galileo Gen1 or Gen2][8]



##VS Solution

* Open the Visual Studio Solution for the Intel Galileo  sample, located in the repo @ \Devices\IntelGalileo\ConnectTheDotsGalileo.sln

##Configuration

* Open the ConnectTheDotsGalileo.exe.config file and edit the appSettings used to configure the AMQPS connection to Event Hub using the information received from setting up the Azure services using the ConnectTheDotsCloudDeploy tool. For the deviceName, pick one of your choosing.  DeviceID can also be a value of your choosing, but should be unique if you plan to deploy multiple sensors against your Azure service.

You can retrieve the Host, User, and Password strings by 
  
1. launching http://manage.windowsazure.com 
2. selecting Service Bus in the left nav menu 
3. picking your Namespace (used for `Host`)
4. select Event Hubs from the top menu
5. select ehdevices (this value is used for `Path` but should not need to be modified)
6. select Connection Information tab at the bottom (used for `User` and `Password`, 'Name' can be used for `User` value and the 'SharedAccessKeyName' in "Connection String" can be used for `Password`, Note: all passwords end with '=')


```
<configuration>

  <appSettings>

    <add key ="DeviceName" value="galileo"/> 
    <add key ="DeviceID" value="galileo"/>
    <add key ="Host" value="{service bus namespace}.servicebus.windows.net"/>
    <add key ="Path" value="ehdevices"/>
    <add key ="User" value="D1"/>
    <add key ="Password" value="{shared access key}"/>
  </appSettings>

</configuration>
```

An example config can be seen below, note that the `Password` key is given a placeholder value for security reasons.

![ScreenShot](http://i.imgur.com/hGUJ1l2.png)

* Open ConnectTheDotsGalileo => Source Files => Main.cpp and note the `#defines`

	// The code below can be used to send simulated data if you don't have a weather shield  
	// #define SIMULATEDATA  
	// #define USEONBOARDSENSOR  
	// #define USEGROVESTARTERKIT  
	// #define USESPARKFUNWEATHERSHIELD

Simply uncomment (remove the //) for the #define that describes your sensor

For example, to enable the use of the Sparkfun Weathershield, see the modification made below.


![ScreenShot](http://i.imgur.com/IZsZ1QH.png)

* Once you have applied these changes to the code, connect your chosen sensor to the Galileo board

* Connect the Ethernet port of your Galileo to an Ethernet cable connected to a network with DHCP, and ensure your device is connected to the same network as your development machine

![ScreenShot](http://i.imgur.com/p6vRZXW.jpg)

* When hitting Ctrl+F5 or the 'Remote Windows Debugger' button in Visual Studio, the Galileo board will receive your program through a remote deployment and will start sending its temperature and humidity data to Azure services.

You can view diagnostic data in the Output Windows (View => Output or Ctrl+W,O) to verify that your data is sending successfully

![ScreenShot](http://i.imgur.com/RZBaU4q.png)

##Tips

* See the [Advance Usage guide for Galileo][9] for details on setting up a Wi-Fi ethernet adapter and more!

* For outdoor placement, ensure that your electronics are well protected.  In this exampl,e the Galileo Gen 2 board has a Seeed Grove with temperature and light sensors protected by a plastic bag running outside a closed window.

![ScreenShot](http://i.imgur.com/CKg6qNg.png)

* Making your Galileo run an exe on boot
	1. From a file explorer window, navigate to \\mygalileo\c$\Windows\System32\Boot
	2. If prompted enter the username as \Administrator and the password as admin
	3. Right click on autorun.cmd and select Edit
	4. At the end of the file add: start YourAppLocation\YourAppName.exe i.e. 'start c:\test\ConnectTheDotsGalileo.exe'  


##Troubleshooting

* `Unable to connect to the Microsoft Visual Studio Remote Debugging Monitor named 'mygalileo`

Verify that your device is on the same network as your development machine.  If you still have issues, you may need to modify the Remote Debugging Settings by right-clicking `ConnectTheDotsGalileo` in the Solution Explorer => Properties => Configuration Properties => Debugging

Here you can change the `Remote Server Name` to another value if you configured your board differently, or use the device ip if your network has issues resolving the Hostname of your Galileo device.

![ScreenShot](http://i.imgur.com/7k1omJW.png)

* `Unable to deploy local file '\Devices\IntelGalileo\Debug\ConnectTheDotsGalileo.exe' (remote file path 'c:\test\ConnectTheDotsGalileo.exe')` 

	*May include similar error for libeay32.dll, ssleay32.dll, and qpid-proton.dll*

Verify that ConnectTheDots.exe is not already running on the device, if it is, kill the process and retry.  This is common if you auto-execute the program at boot time. 

To remedy, telnet in to your device by opening a command prompt and typing 'telnet mygalileo', login with your credentials, type 'tlist', then 'kill' followed by the process number of the ConnectTheDots.exe

Example:  

![ScreenShot](http://i.imgur.com/TgVUOc2.png)

* `Unable to view real-time data in ConnectTheDots website deployment`

Verify that data is sending successfully by monitoring the output of your device in Visual Studio.

You can view diagnostic data in the Output Windows (View => Output or Ctrl+W,O) to verify that your data is sending successfully.

![ScreenShot](http://i.imgur.com/RZBaU4q.png)



  [1]: http://www.amazon.com/gp/product/B00K53CQK4/
  [2]: http://www.amazon.com/gp/product/B00MF753JY
  [3]: http://www.amazon.com/gp/product/B00H8OI1RU
  [4]: http://www.amazon.com/gp/product/B00AJKY8TK/
  [5]: http://www.amazon.com/gp/product/B00AF22GDC/
  [6]: https://www.windowsondevices.com/signup.aspx
  [7]: http://ms-iot.github.io/content/SetupPC.htm
  [8]: http://ms-iot.github.io/content/SetupGalileo.htm
  [9]: http://ms-iot.github.io/content/AdvancedUsage.htm

