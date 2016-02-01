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
   azure group create -n "OlivierCTDRG" -l "East US"
   ```
1. Deploy the solution typing the following command:
   ```
   azure group deployment create -f "C:\IoT\connectthedots\Azure\ARMTemplate\ConnectTheDots.json" OlivierCTDRG OlivierCTDDeploy 
   