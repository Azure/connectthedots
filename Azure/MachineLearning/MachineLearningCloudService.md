Once you have setup devices and Azure Event Hubs to receive data from your devices, you can do some analytics on the data.
Azure Machine Learning offers a wide range of APIs, along with a platform to create advanced machine learning algorithm.
In this example we use the Anomaly Detection API that is available on the [Azure Data Market](http://datamarket.azure.com/dataset/aml_labs/anomalydetection).

In order to implement the sample you will need to do one of the following (the instructions below assume you are doing the first one):

* [Sign up](http://datamarket.azure.com/checkout/f33b2da0-af7c-42dd-85eb-d625e688f876?ctpa=False) for the free trial (that gives you up to 25,000 transactions per month)
* Create your own Machine Learning endpoint (we are not covering this here, but you can learn how to do so [here](http://azure.microsoft.com/en-us/services/machine-learning/))

The sample consist in a Cloud Service that reads real time data from ehDevices Event Hub and calls the Machine Learning Anomaly Detection API, then sends alerts triggered by the API to the ehAlerts Event Hub so that Web sites and clients can display the alerts.

## Prerequisites ##

Make sure you have all software installed and necessary subscriptions as indicated in the Readme.md file for the project. To repeat them here, you need

1. Microsoft Azure subscription ([free trial subscription](http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)
1. If you have not done so already, [Sign up](http://datamarket.azure.com/checkout/f33b2da0-af7c-42dd-85eb-d625e688f876?ctpa=False) for the Anomaly Detection API service.
1. Visual Studio 2013 – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs)

In addition, you must have run the AzurePrep program discussed in that section, as it creates the event hubs from which the Cloud service pulls data. If you already have the event hubs, you can find information needed below in your Azure portal (see below)

## Deploy the Cloud Service ##

* Open the ConnectTheDots\Azure\AzurePrep\AzurePrep.sln solution in Visual Studio.
* Open and edit the configuration file WorkerHost\app.config:
    * Find the lines for the Connection Strings (look for Microsoft.ServiceBus.ConnectionString, Microsoft.ServiceBus.ConnectionStringDevices and Microsoft.ServiceBus.ConnectionStringAlerts)
   * Replace the connection strings with the appropriate values for your subscription, found in [https://manage.windowsazure.com](https://manage.windowsazure.com) as follows:
         * **ServiceBus.ConnectionString**. Select Service Bus from the left nav menu, highlight the Namespace Name created earlier, click on Connection Information at the bottom of the screen, and copy the RootManagedSharedAccessKey.
         * **ServiceBus.ConnectionStringDevices**. Select Service Bus from the left nav menu, select the Namespace Name created earlier, highlight ehdevices, click on Connection information at the bottom of the screen, and copy the WebSite Connection string.
         * **ServiceBus.ConnectionStringAlerts**. Select Service Bus from the left nav menu, select the Namespace Name created earlier, highlight ehalerts, click on Connection information at the bottom of the screen, and copy the WebSite Connection string.
   * Find the lines for the Anomaly Detection settings and edit appropriately
      * **AnomalyDetectionApiUrl**:
         * if you are using the data market API, keep the URL unchanged (https://api.datamarket.azure.com/aml_labs/anomalydetection/v1/)
         * If you are using your own Machine Learning endpoint use its URL
      * **AnomalyDetectionAuthKey**:
         * If you are using the data market API, go to [https://datamarket.azure.com/account](https://datamarket.azure.com/account) (login in with the account used to subscribe to the Anomaly Detection API) and search for "Primary Account Key"
         * If you are using your own Machine Learning endpoint use the authentication key provided.
      * **LiveId**:
         * If you are using the data market API, use the Windows ID you used to subscribe to the API
         * If you are using your own Machine Learning endpoint this parameter is ignored
      * **UseMarketApi**:
         * If you are using the data market API, leave this one unchanged ("true")
         * If you are using your own Machine Learning endpoint, set to "false".
   * The settings **TukeyThresh** and **ZScoreThresh** can be adjusted to fine tune the Anomaly Detection algorithm.
   * The **AlertsIntervalSec** sets the minimum time in seconds between 2 alerts from the Anomaly Detection algorithm

* You can test the service locally by hitting F5 in Visual Studio 
* If you have created and published a website using the samples in this project, you should see anomalies detected in the charts and in the alerts table when sensors data changes.
* To deploy the Cloud Service, right click on the WorkerRole project, click on "Publish" and follow the prompts

