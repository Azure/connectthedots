## Generic Web Service to Event Hub scenario ##

Generally speaking, you will have devices which you want to connect to Azure, and the examples in the [Getting Started ](https://github.com/Azure/connectthedots/blob/master/GettingStarted.md ) section of [Connect The Dots ](https://github.com/Azure/connectthedots ) and in the various subdirectories show you one way to do this. The best way would be to use Microsoft's [IoT Suite](https://azure.microsoft.com/en-us/solutions/iot-suite/). 

However, there are times when you do not have access to the devices or data producers, for example when you want to use public data feeds (such as the Department of Transportation's traffic information feed) as your data source. In this case you do not have the ability to put any code on the device or remote gateway to push the data to an Azure Event Hub or IoT Hub; rather, you need to set up something to pull the data from those sources and then push it to Azure. The simplest way is to run an application in Azure to do this. The code in this project is an example of how to do this. It is not a supported solution, or even a recommended one, but simply an example.


## Prerequisites ##

* An Azure subscription. In order to configure and deploy the application you will need to have set up a Service Bus namespace and an Event Hub. An easy way to do this is to use [AzurePrep](https://github.com/Azure/connectthedots/tree/master/Azure/AzurePrep ), in the [ConnectTheDots](http://connectthedots.io ) open source project, but that is not a prerequisite - set up the Event Hub manually if you like. Just make sure to configure at least one Shared Access Policy for the event hub.
* A version of Visual Studio installed on your desktop.
* Access to a source of data - either a public source not requiring credentials, or a private source and access credentials   

## Setup Tasks ##

Setting up the application once you have an Event Hub and its Connection String involves the following tasks, which will be described in greater detail below.

1. Clone or copy the project to your machine 
2. Open the project solution in Visual Studio
3. Edit App.config in the Worker Host folder to provide the relevant source and target configuration data
4. Build the project
5. Publish the application to your Azure subscription
6. Verify that data is coming in to your Event Hub from the public data source.


## Editing app.config ##

There are two sections of App.config you will need to change, appSettings and XMLApiListConfig.

In the appSettings section, at a minimum enter

* The **connection string to your Service Bus**. To find it, in the original [Azure management portal](http://manage.windowsazure.com), select Service Bus from the left nav menu, highlight your Namespace Name in the right pane, and click on Connection Information at the bottom of the page. In the Access connection information window that opens, highlight and copy the Connection String shown. It should look something like the following:   
```
Endpoint=sb://myservicebusname.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Axf5bbXYZeEaLoIeLMN2RV1sc3HdkYxFq7RX/T6a5TE=
```
Find the appSettings section in App.Config, and the line that says
```
<add key="Microsoft.ServiceBus.ServiceBusConnectionString" value="[Service Bus connection string]" />
```
Replace [Service Bus connection string] with the whole of the string that you copied from the management portal, starting with "Endpoint" and ending with an "=".

* The **name of the Event Hub** to which you want the data sent. Find the line in the appSettings section that says
```
<add key="Microsoft.ServiceBus.EventHubToUse" value="[event hub name]" /> 
```
Replace [event hub name] with the name of the event hub that you created.

* The **XML to JSON conversion flag**. If your data source sends you JSON formatted data, you are fine and do not need to change anything. On the other hand, if it sends you XML rather than JSON, you need the application to convert the XML into JSON before sending it to your event Hub. Find the line that says
```
<add key="SendJson" value="false" />
```
and change 'false' to 'true'.

* The **credentials for the web service** from which you will pull the data. If the site is a public site and does not need access credentials, you do not need to do this. If you do need the application to send credentials to the web service, find the section
```
    <appSettings>
      <add key="UserName" value="[Api user name]" />
      <add key="Password" value="[Api password]" />
    </appSettings>
```
Replace the [Api user name] with your user name, and the [Api password] with your password. It would look something like this:
```
    <appSettings>
      <add key="UserName" value="Myname" />
      <add key="Password" value="Mypassword" />
    </appSettings>
```

In the XMLApiListConfig section, at a minimum enter

* The **URL for the web service** from which you will pull the data. Find the section

	    <XMLApiListConfig>
	      <add APIAddress="https://api/last"/>
	    </XMLApiListConfig>

Replace the APIAddress section with one or more URLs for web services you will access (the application will cycle through this list). It would look something like this if you have three URLs to access from the same root location:

    <XMLApiListConfig>
      <add APIAddress="https://www.somepublicdatasource.com/feed1"/>
      <add APIAddress="https://www.somepublicdatasource.com/feed2"/>
      <add APIAddress="https://www.somepublicdatasource.com/feed3"/>
    </XMLApiListConfig>

## Publishing the application ##

1. In Visual Studio, right-click on 'DeployWorker' in Solution 'GenericWebToEH'/Azure, and select *Publish*.
2. In the Publish Azure Application, answer the following questions. 
    * Name: [pick something unique]
    * Region: [pick same region as you used for the Event Hub]
    * Database server: no database
    * Database password: [leave suggested password]
3. Click Publish, and wait until the status bar shows "Completed". At that point the application is running in your subscription, polling the web sites you listed for the data they publish, and pushing it to your Event Hub. From there, you can access with Stream Analytics or any other application as you would normally.
