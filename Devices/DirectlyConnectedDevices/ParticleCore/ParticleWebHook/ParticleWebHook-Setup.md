## Particle WebHook Setup ##

1. Ensure you have completed setting up the [ConnectTheDots EventHub Deployment](https://github.com/toolboc/connectthedots/blob/master/Azure/AzurePrep/AzurePrep.md)
2. [Install the Particle CLI tool](http://support.particle.io/hc/en-us/articles/203265730-Installing-the-Particle-CLI) on your operating system of choice (You may skip the portion on enabling DFU-Util)
3. Create a new file named webhook.json with a structure similar to the following (See: [Particle Webhooks Documentation](http://docs.particle.io/photon/webhooks/)):


{
    
"event": "ConnectTheDots",

    "url": "https://YOUR_EVENT_HUB_NAME-ns.servicebus.windows.net/ehdevices/messages",
    "requestType": "POST",
    "json": {
        "subject": "{{s}}",
        "unitofmeasure": "{{u}}",
        "measurename": "{{m}}",
        "value": "{{v}}",
        "organization": "{{o}}",
        "displayname": "{{d}}",
        "location": "{{l}}",
        "timecreated": "{{SPARK_PUBLISHED_AT}}",
        "guid":  "{{SPARK_CORE_ID}}"
    },
    "azure_sas_token": {
        "key_name": "D1",
        "key": "YOURKEYHERE"
    },
    "mydevices": true
}


Note: There is a max send size of 255 characters from the Particle Core Device, please keep this in mind when naming variables!  The `azure_sas_token` is very important as it is used server-side by the particle.io WebHooks service to appropriately forward to your Event Hub REST API. 

4. Modify `YOUR_EVENT_HUB_NAME`, and `key` to match the value from your Event Hub.
5. Launch the Spark CLI tool (Open CMD prompt on Windows) and type “particle login” then login with your particle.io credentials
6. Navigate to the folder containing webhook.json and type “particle webhook create webhook.json”
7. Verify your webhook was created with “particle webhook list”
8. Now in the Particle Web IDE you can send data to your Azure Event Hub using “Spark.publish(`NAME_OF_YOUR_EVENT`, payload);”
