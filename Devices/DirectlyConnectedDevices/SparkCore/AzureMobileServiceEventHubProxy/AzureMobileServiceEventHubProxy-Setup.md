## Azure Mobile Service Event Hub Proxy Setup ##
The [Azure Event Hubs REST API](https://msdn.microsoft.com/en-us/library/azure/dn790664.aspx) requires sending messages over HTTPS. However, our Spark Core device has minimal CPU and library support for creating HTTPS/TLS connections and lacks libraries for creating the necessary SAS token.  We will create a proxy within an Azure Mobile Service to forward our regular HTTP payload over to our Event Hub with the proper HTTPS encoding and token response.

## Software Requirements ##
[Node.JS for Windows](https://nodejs.org/)

[Git for Windows](http://www.git-scm.com/downloads)

## Create the Mobile Service ##
Create a new Azure Mobile Service with a Javascript Backend:

![Create the Azure Mobile Service](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/CreateAMS.png)

![Select the Javascript Backend](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/AMS-JS-Option.png)

Next Create an API within the service, I named mine 'temp' for temperature:

![Create an API within the Azure Mobile Service](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/AMS-API.png)

## Add Custom JS Modules to the newly created API ##

Let’s begin, by building the Event Hub Proxy in the “temp” API we just created.  This API will require Custom Node.JS packages that can be installed by following Redbit’s [“Using Custom NodeJS Modules with Azure Mobile Services“](http://www.redbitdev.com/using-custom-node-js-modules-with-azure-mobile-services/).  Follow the instructions and be sure to run an npm install for https, crypto, and moment as these are required to generate the SAS Key for sending data through the Event Hub rest Service.

## Modify the Azure Mobile Service API Script Code ##

After following the step above and checking in the addition of https, crypto, and moment dependencies.  Head back to your Azure Portal and edit the API code for your newly created API with the following:

[Azure Mobile Service Event Hub Proxy - Gist](https://gist.github.com/toolboc/55030fd88bc6412ce115)

Be sure to modify the values for:

// Event Hubs parameters

var namespace = 'EVENTHUBNAMESPACE';

var hubname ='EVENTHUBNAME';

AND

// Shared access key (from Event Hub configuration) 

var my_key_name = 'KEYNAME'; 

var my_key = 'KEY';

![Modify the Azure Mobile Service API Code](https://github.com/MSOpenTech/connectthedots/blob/master/Wiki/Images/AMS-API-Code.png)
