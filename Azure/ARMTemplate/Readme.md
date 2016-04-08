To deploy the ConnectTheDots solution to your Azure Subcsrition you will need to follow the below instructions.
We are using Azure Resource Manager to deploy the needed services and connect them to one another. We will utilize the Azure cross platform CLI tool which will allow you to deploy the sercices from your favorite development machine, running Windows, Linux or OSX.

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
1. Deploy the solution typing the below command. Note that you will be prompted for the following:

   * iothub SKU:  you can seelct F1 (free), S1 or S2 (see [here](https://azure.microsoft.com/en-us/pricing/details/iot-hub/) for details on pricing for these skus). Note that only 1 instance of the free SKU of IoT Hub is allowed per Azure subscription.
   * solution name: this name has to be **all lower case** and **less than 16 characters**

   ```
   azure group deployment create -f "ConnectTheDots.json" ConnectTheDotsRG ConnectTheDotsDeploy 
   ```
   