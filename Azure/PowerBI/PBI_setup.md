The instructions below will help you setup a Power BI dashboard in the Connect The Dots starter solution, but they can be adapted as necessary for other scenarios. This document assumes you have already deployed the starter solution with at least one device set up pushing data to an Azure IoT Hub. The documentation below uses the names and fields you would have if you had set up the starter solution with simple device connected and sending Temperature and Humidity data, but can be modified as needed if you have a different sensor or named your fields and hubs differently.

## Prerequisites ##

Make sure you have a working starter solution, with data showing in your Azure website. In addition, you will need a Power BI account, for which you can sign up for at [PowerBI.com](http://www.PowerBI.com).

## Create a new Consumer Group in your IoT Hub ##

To make sure you do not exceed the maximum number of readers on your Connect The Dots Event Hub, create a Consumer Group first.

* Open the [Azure Management Portal](https://portal.azure.com), and select the resource group of your Connect The Dots solution.
* Find and select the IoT Hub instance
* In the IoT Hub settings blade, click on Endpoints in the MESSAGING section
* Select the "Events" built-in endpoint
* In the properties blade of this endpoint, add a new Consumer Group called "cg4pbi" (remember to click on "Save" at the top to save the change)


## Create an Azure Stream Analytics (ASA) job ##

* In the [Azure Management Portal](https://portal.azure.com), go back to the resource group for your Connect The Dots solution deployment.
* Click on the "+Add" button on top of the Resource Group view
* Click on "Stream Analytics Job"
* Click on "Create" at the bottom
* Type in the name for the job (we'll assume you used "CTD2PBI" as a name). Ensure you are creating the job in the same resource group (which should be the default and will make it easier to find it in the portal)
* Go back to the resource group view and select the CTD2PBI Stream Analytics job.
* Add IoT Hub as the Input
	* Click on the "Inputs" box
	* Click on "+Add"
		* Input Alias: “DevicesInput”
		* Source type: "Data Stream"
		* Source: "Iot Huyb"
		* Import Option: "Use IoT Hub form current subscription"
		* IoT hub: _The name of the IoT hub of your Connect The Dots solution_
		* Enpoint: "Messaging"
		* Shared access policy name: "iothubowner"
		* Consumer Group: "cg4pbi"
		* Event Serialization format: JSON
		* Encoding: "UTF-8"
* Create a query 
    * Click on the "Query" box in the Stream Analytics blade
    * Copy/paste contents “cg4pbi.sql” found in the ConnectTheDots\Azure\StreamAnalyticsQueries folder of the repository
    * Save
* Create an output to send data to PowerBI
	* Click on the "Outputs" box
	* Click on "+Add"
		* Output Alias: "CTDPBI"
		* Sink:  "Power BI"
		* Click on "Authorize" if asked to and enter your Power BI account credentials
		* Group Workspace: "My Workspace"
		* Dataset Name: "CTDPBIDataSet"
		* Table Name: "CTDPBITable"
* Start the Stream Analytics Job
    * Go back to the Stream Analytics CTD2PBI job blade
	* Click on "Start" (top menu)
	* Select "Now" and click on "Start" (bottom button)

## Create a Power BI dashboard ##
###Create a dashboard###
We are going to create a Power BI dashboard for a the data coming from the Connect The Dots starter solution, of a single Arduino UNO + Weather Shield sending data to an Azure Event Hub.

To create this, first create a dashboard:

* Log in to [http://App.PowerBI.com](http://app.powerbi.com)
* Create a Dashboard for your Connect The Dots data
	* Click "+" in the left menu under Dashboards
	* Enter a name: "ConnectTheDots"

###Create a simple line chart###
The first chart on your dashboard will be a real-time timeline showing the temperature from your sensor. To create this, follow the steps below in order.
Note that you need data flowing from at least one device for the Power BI dataset to be created. Start a device and wait a couple minutes before moving on to make sure you will see the dataset created and fed by the Stream Analytics job.

* In the upper menu of the ConnectTheDots dashboard, click on "+ Add Tile"
* Select "Custom Streaming Data" and click "Next"
* Select CTDPBIDataSet data set and click "Next"
* Select Following settings:
	* Visualization Type = "Line Chart"
	* Axis : "timecreated"
	* Legend : "measurename"
	* Values : "value"
	* Time window to display : 1 minute

You now have a simple dashboard showing real time data coming from devices in PowerBI.
You can now consider customizing the query in stream analytics and the dashboard to show things like average, alerts,...
Enjoy!




