# Stream Analytics Setup #
The instructions below will help you setup the Stream Analytics queries in the Connect The Dots getting started project, but they can be adapted as necessary for other scenarios. This document assumes you have all the necessary software and subscriptions and that you cloned or downloaded the ConnectTheDots.io project on your machine.

## Prerequisites ##

Make sure you have all software installed and necessary subscriptions as indicated in the ReadMe.md file for the project. To repeat them here, you need

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. Visual Studio – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs)

Note also that these queries are hard-coded to the data streams defined in the getting started walkthrough in this project, meaning the same JSON string contents, etc. Also note that the SQL queries ARE CASE SENSITIVE, so that "temperature" <> "TEMPERATURE". You should make sure that the spelling and case of the incoming measure names are the same as in the SQL queries.

## Create three Azure Stream Analytics (ASA) jobs ##

* If you have used the ARM template to deploy the ConnectTheDots solution, then you can edit the Stream Analytics job directly in the portal, looking for it in the Resource Group created during the deployment of the solution.
* If you are creating a new job, read this:
    * Open the [Azure Management Portal](http://portal.azure.com), and create a new job “Aggregates”:
        * "+” in top left corner > Internet Of Things > Stream Analytics >
            * Job name: “Aggregates”.
            * Subscription: same as the one used for the other parts of the solution.
            * Resource Group: same as the one used for the other parts of the solution.
            * Location: your choice, considering it is always better to have the various services of a solution in the same region.
            * Click on Create
    * Create an input
        * In the Resource Groups list, select your solution's resource group.
        * Select the stream analytics job "Aggregates"
        * Click on the Inputs tile in the Aggregates job.
        * *Inputs blade > Add >*
            * Input Alias: “DevicesInput”
            * Source Type: "Data Stream"
            * Source: "IoT Hub"
            * Subscription: pick the current subscription
            * IoTHub: pick the IoT Hub named out of your solution name (captured during the deployment of the ARM template)
            * Shared access policy name: "iothubowner"
            * Event serialization format: "JSON"
            * Encoding: "UTF-8"
    * Create a query 
        * Select the Query tile in the Aggregates job blade
        * Copy/paste contents `Aggregates.sql` found in the `ConnectTheDots\Azure\StreamAnalyticsQueries` folder in Windows Explorer
        * Save
    * Create an output
        * Select the Output tile in the Aggregates job blade
        * *Output tile > Add >*
            * Output Alias: your choice
            * Sink: "Event Hub"
            * Subscription: pick the current subscription
            * Service bus namespace: Pick the one named after the solution name you entered during the deployment of the ARM template
            * Event Hub Name: "ehalerts"
            * Event Hub policy name: "RootManageSharedAccessKey"
            * Event Serialization format: "JSON"
            * Encoding: "UTF-8"
            * Click on Create
        * **Note** You will likely get an error just about the same container being used as input and output. This is OK, the job will still work.

    * Start the Job
        * *Dashboard > Start* on the bottom bar.
* Create a second job “Alerts”: as above, but use `alert.sql` contents for the query, and use *ehalerts* for the Output Event Hub, not *ehdevices*.
* Create a third job “LightSensor”: as above, but use `lightsensor.sql` contents for the query, and use *ehalerts* for the Output Event Hub.

Once all three are running, go check out your site at http://`<yourURL>`.azurewebsites.net/.

### What these streams do ###
Now that you have them set up, a quick explanation of what each one does would be helpful.

**Aggregates** job gets the data from the temperature sensor, and creates the average within a given window. This allows us to chart the rolling average on top of the temperature chart.

**Alerts** keeps tabs on the temperature max, and creates an alert if the temperature rises above 80, on both the raw data and the average coming from the Aggregates stream.

**Light Sensor** issues an alert when the lights are turned off, which in the query we have provided, is when the lumen value is under 0.02.

For more details on the website and what it shows, check out the [Website Details](../WebSite/WebsiteDetails.md).

