To deploy the ConnectTheDots solution to your Azure Subcsrition you will need to follow the below instructions.
We are using Azure Resource Manager to deploy the needed services and connect them to one another.
We are also using the Azure cross platform CLI tool which will allow you to deploy the services from your favorite development machine, running Windows, Linux or OSX.
The below services will be deployed in your Azure subscription:
   - 1 instance of Azure IoT Hub (using the SKU of your choice, considering you can only deploy 1 instance of the free SKU per subscription)
   - 1 Storage account (Standard performance)
   - 1 Service Bus instance (Basic tier) with 1 Event Hub (1 throughput Unit)
   - 1 Stream Analytics Job (1 streaming unit)
   - 1 App Service plan (Standard: 2 Small SKU) with 1 Web app
   
You can edit the [ARM template](ConnectTheDots.json) if you want to add more services or edit the parameters.

**If you want to make edits to the WebSite before deploying the solution** with the ARM template, here are the few steps you can go through:
1. Do your changes in the WebSite solution (note that if you want to debug along with actual other services (IotHub, and others), you will need to first deploy the ARM Template.
1. Export a WebDeploy package (you can find instructions on how to do this from the WebSite project [here](https://msdn.microsoft.com/en-us/library/dd465323%28v=vs.110%29.aspx).
1. Edit the [ARM template](ConnectTheDots.json), updating the packageUri defaultvalue to point to your local zip package (see [this article](https://blogs.perficient.com/microsoft/2016/03/deploy-azure-web-app-using-arm-template-from-visual-studio-2015/) for details on how to do this).

Now here is how to deploy the whole ConnectTheDots solution in a few command lines:

1. Install the Azure CLI tool following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-install/).
1. Connect to Azure following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/).
1. If you have multiple subscriptions, select the one you want to deploy the solution to following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/#multiple-subscriptions) 
1. Set the Resource Azure Manager mode typing the following command:
   ```
   azure config mode arm
   ```
1. Create a new resource group typing the following command:
   ```
   azure group create -n "ConnectTheDotsRG" -l "East US"
   ```
1. Navigate to the Azure\ARMTemplate folder in the repo
   ```
   cd C:\My\Repo\Location\Azure\ARMTemplate
   ```
1. Deploy the solution typing the below command. You will actually be prompted for the following:

   * iothub SKU:  you can select F1 (free), S1 or S2 (see [here](https://azure.microsoft.com/en-us/pricing/details/iot-hub/) for details on pricing for these skus). Note that only 1 instance of the free SKU of IoT Hub is allowed per Azure subscription.
   * solution name: this name has to be **all lower case** and **less than 16 characters**

   ```
   azure group deployment create -f "ConnectTheDots.json" ConnectTheDotsRG ConnectTheDotsDeploy 
   ```
1. If you are seeing errors during the deployment, you can diagnose following instructions on how to debug ARM deployments: [http://aka.ms/arm-debug](http://aka.ms/arm-debug).
