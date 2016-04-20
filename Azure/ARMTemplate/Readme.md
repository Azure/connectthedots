##How to deploy Azure services for ConnectTheDots.io##
To deploy the ConnectTheDots solution to your Azure subscription you will need to follow the below instructions.
We are using Azure Resource Manager to deploy the needed services and connect them to one another.
We are also using the Azure cross platform CLI tool which will allow you to deploy the services from your favorite development machine, running Windows, Linux or OSX.
The below services will be deployed in your Azure subscription:
   - 1 instance of Azure IoT Hub (using the SKU of your choice, considering you can only deploy 1 instance of the free SKU per subscription)
   - 1 Storage account (Standard performance)
   - 1 Service Bus instance (Basic tier) with 1 Event Hub (1 throughput Unit)
   - 1 Stream Analytics Job (1 streaming unit)
   - 1 App Service plan (Standard: 2 Small SKU) with 1 Web app
   
You can edit the [ARM template](ConnectTheDots.json) if you want to add more services or edit the parameters.

##Your connect the dots resource groups##
All the services will be deployed under a single resource group in Azure.
The [Azure resource groups](https://azure.microsoft.com/en-us/updates/resource-groups-in-azure-preview-portal/) are a concept allowing to manage a set of resources all together.
This allows you to easily find the various resources for your ConnectTheDots solution in the Azure portal.

##Editing the deployment ARM template##
The default ARM template doesn't require editing unless you want to change the architecture of your solution to go from the default ConnectTheDots one to your own version of it.

##Deploy using Azure CLI tool and the ARM template##
Now here is how to deploy the whole ConnectTheDots solution in a few command lines:

1. Install the Azure CLI tool following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-install/).
1. Connect to Azure following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/).
1. If you have multiple subscriptions, select the one you want to deploy the solution to following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/#multiple-subscriptions) 
1. Set the Resource Azure Manager mode typing the following command:
   ```
   azure config mode arm
   ```
1. Create a new resource group typing the following command (you can replace "ConnectTheDotsRG" with the name of your choice for the resource group):
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

##Deleting a ConnectTheDots solution from your Azure subscription##
You can easily delete all the Azure resources at once when you are done with your project and want to clean up your Azure subscirption.
You can do this using a command line in the Azure CLI tool or in the Azure portal.

###Delete the resources using Azure CLI###
If you are already logged in in the Azure CLI tool, go directly to step #4

1. Connect to Azure following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/).
1. If you have multiple subscriptions, select the one you want to deploy the solution to following the instructions [here](https://azure.microsoft.com/en-us/documentation/articles/xplat-cli-connect/#multiple-subscriptions) 
1. Set the Resource Azure Manager mode typing the following command:
   ```
   azure config mode arm
   ```
1. Delete the resource group typing the following command (you need to replace "ConnectTheDotsRG" with the name you used in step 5 of the deployment if you changed it):
   ```
   azure group delete -n "ConnectTheDotsRG"
   ```
