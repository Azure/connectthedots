## Generic Web Service to Event Hub scenario ##

Generally speaking, you will have devices which you want to connect to Azure, and the examples in the [Getting Started ](https://github.com/Azure/connectthedots/blob/master/GettingStarted.md ) section of [Connect The Dots ](https://github.com/Azure/connectthedots ) and in the various subdirectories show how to do this. 

However, there are times when you do not have access to the devices or data producers, for example when you want to use public data feeds (such as the Department of Transportation's traffic information feed) as your data source. In this case you do not have the ability to put any code on the device or remote gateway to push the data to an Azure Event Hub or IoT Hub; rather, you need to set up something to pull the data from those sources and then push it to Azure. The simplest way is to run an application in Azure to do this.

For this, and only this scenario, you should use the [GenericWebToEH](https://tbd) application. 

## Prerequisites ##

In order to configure and deploy the GenericWebToEH application you will need to have set up an Event Hub and know the Connection String. The easiest way to do this is to use [AzurePrep](https://github.com/Azure/connectthedots/tree/master/Azure/AzurePrep ), but that is not a prerequisite - set up the Event Hub manually if you like.

## Setup Tasks ##

Setting up the application once you have an Event Hub and its Connection String involves the following tasks, which will be described in greater detail below.

1. Clone or copy the project to your machine 
2. Open the `ConnectTheDots\Azure\GenericWebToEH\GenericWebToEH.sln` solution in Visual Studio
3. Edit App.config in the Worker Host folder to provide the following
	1. The connection string to your Event Hub
	2. The URL for the web service from which you will pull the data
	3. The credentials you will use for accessing the web service with the data
4. Build the project from the *BUILD* menu (Select Release, not Debug)
5. Publish the application to your Azure subscription
6. Verify that data is coming in to your Event Hub from the public data source.



## Editing app.config ##

There are three sections of App.config you will need to change

**The connection string to your Event Hub**. Find the string


	    * <TargetAMQPConfig AMQPSAddress="amqps://[key-name]:[key]@[namespace].servicebus.windows.net" EventHubName="ehdevices" EventHubMessageSubject="gtsv" EventHubDeviceId="a94cd58f-4698-4d6a-b9b5-4e3e0f794618" EventHubDeviceDisplayName="SensorGatewayService" />

Replace the AMPSAddress string with the connection string for your Event Hub, and the word 'ehdevices' with the name of your Event Hub. It should look something like this:

	    * <TargetAMQPConfig AMQPSAddress="amqps://D1:XAP3M6+IZZZHhE1n4iFhclg55Anpz3d6P/Fk5j56j/k=@mynamespace-ns.servicebus.windows.net" EventHubName="ehmine" EventHubMessageSubject="gtsv" EventHubDeviceId="x94ca58g-4555-4d6a-b6b1-4e3e0f554615" EventHubDeviceDisplayName="SensorGatewayService" />


**The URL for the web service from which you will pull the data**. Find the section

	    <XMLApiListConfig>
	      <add APIAddress="https://api/last"/>
	    </XMLApiListConfig>

Replace the APIAddress section with one or more URLs for web services you will access (the application will cycle through this list). It would look something like this if you have three URLs to access from the same root location:

    <XMLApiListConfig>
      <add APIAddress="https://www.msconnectthedots.com/feed1"/>
      <add APIAddress="https://www.msconnectthedots.com/feed2"/>
      <add APIAddress="https://www.msconnectthedots.com/feed3"/>
    </XMLApiListConfig>


**The credentials for the web service from which you will pull the data**. Find the section

    <appSettings>
      <add key="UserName" value="[Api user name]" />
      <add key="Password" value="[Api password]" />
    </appSettings>

Replace the [Api user name] with your user name, and the [Api password] with your password. It would look something like this:

    <appSettings>
      <add key="UserName" value="Myname" />
      <add key="Password" value="Mypassword" />
    </appSettings>


You can also change the key "SendJson" from "false" (in which case it sends the data to Event Hub exactly as it gets it from the URL) to "true" if you need to translate from one format (e.g. XML in this example) to JSON. Any translation issues you need to address should be done in that section.



## Publishing the application ##

* In Visual Studio, right-click on 'DeployWorker' in Solution 'GenericWebToEH'/Azure, and select *Publish*.
* In the Publish Azure Application, answer the following questions. 
    * Name: [pick something unique]
    * Region: [pick same region as you used for the Event Hub]
    * Database server: no database
    * Database password: [leave suggested password]
* Click Publish, and wait until the status bar shows "Completed". At that point the application is running in your subscription, polling the web sites you listed for the data they publish, and pushing it to your Event Hub. From there, you can access with Stream Analytics or any other application as you would normally.
