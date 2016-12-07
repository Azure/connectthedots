// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <AzureIoTHub.h>
#include <AzureIoTUtility.h>
#include <AzureIoTProtocol_MQTT.h>

// Includes and variables for serializing messages
#include <ArduinoJson.h>
#define JSON_BUFFER_SIZE 512
StaticJsonBuffer<JSON_BUFFER_SIZE> jsonBuffer;

// Includes and variables for the DHT22 temperature/humidity sensor
#include <DHT.h>
#define DHTPIN 2        // Digital pin the sensor is connected to
#define DHTTYPE DHT22   // You'd need to change this one if you want to use a DHT1 or DHT21 sensor
DHT dht(DHTPIN, DHTTYPE);

// Find under Microsoft Azure IoT Suite -> DEVICES -> <your device> -> Device Details and Authentication Keys
static const char* deviceId = "[device-id]";
static const char* connectionString = "[Device Connection String]";

static const char* organization = "My Organization";
static const char* location = "My Location";
static const char* tempMeasureName = "Temperature";
static const char* tempUnitOfMeasure = "C";
static const char* hmdtMeasureName = "Humidity";
static const char* hmdtUnitOfMeasure = "%";

static void sendMessage(IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle, const unsigned char* buffer, size_t size)
{
    IOTHUB_MESSAGE_HANDLE messageHandle = IoTHubMessage_CreateFromByteArray(buffer, size);
    if (messageHandle == NULL)
    {
        LogInfo("unable to create a new IoTHubMessage\r\n");
    }
    else
    {
        if (IoTHubClient_LL_SendEventAsync(iotHubClientHandle, messageHandle, NULL, NULL) != IOTHUB_CLIENT_OK)
        {
            LogInfo("failed to hand over the message to IoTHubClient");
        }
        else
        {
            LogInfo("IoTHubClient accepted the message for delivery\r\n");
        }

        IoTHubMessage_Destroy(messageHandle);
    }
}

/*this function "links" IoTHub to the serialization library*/
static IOTHUBMESSAGE_DISPOSITION_RESULT IoTHubMessage(IOTHUB_MESSAGE_HANDLE message, void* userContextCallback)
{
    IOTHUBMESSAGE_DISPOSITION_RESULT result;
    const unsigned char* buffer;
    size_t size;
    if (IoTHubMessage_GetByteArray(message, &buffer, &size) != IOTHUB_MESSAGE_OK)
    {
        LogInfo("unable to IoTHubMessage_GetByteArray\r\n");
        result = IOTHUBMESSAGE_ABANDONED;
    }
    else
    {
        /*buffer is not zero terminated*/
        char* temp = (char*)malloc(size + 1);
        if (temp == NULL)
        {
            LogInfo("failed to malloc\r\n");
            result = IOTHUBMESSAGE_ABANDONED;
        }
        else
        {
          // TODO: add code to react to command
            memcpy(temp, buffer, size);
            temp[size] = '\0';
            LogInfo("Received message from IoTHub: %s\r\n", temp);
            result = IOTHUBMESSAGE_ACCEPTED;
            free(temp);
        }
    }
    return result;
}

void connect_the_dots_run(void)
{
    dht.begin();

    IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle = IoTHubClient_LL_CreateFromConnectionString(connectionString, MQTT_Protocol);
    if (iotHubClientHandle == NULL)
    {
        LogInfo("Failed on IoTHubClient_CreateFromConnectionString\r\n");
    }
    else
    {
#ifdef MBED_BUILD_TIMESTAMP
        // For mbed add the certificate information
        if (IoTHubClient_LL_SetOption(iotHubClientHandle, "TrustedCerts", certificates) != IOTHUB_CLIENT_OK)
        {
            LogInfo("failure to set option \"TrustedCerts\"\r\n");
        }
#endif // MBED_BUILD_TIMESTAMP


        if (IoTHubClient_LL_SetMessageCallback(iotHubClientHandle, IoTHubMessage, NULL) != IOTHUB_CLIENT_OK)
        {
            LogInfo("unable to IoTHubClient_SetMessageCallback\r\n");
        }
        else
        {
            // Prepare json buffer for sending data
            JsonArray& data = jsonBuffer.createArray();
            JsonObject& tempRoot = jsonBuffer.createObject();
            data.add(tempRoot);
            JsonObject& hmdtRoot = jsonBuffer.createObject();
            data.add(hmdtRoot);

            tempRoot["guid"] = (char*)deviceId;
            tempRoot["displayname"] = (char*)deviceId;
            tempRoot["location"] = (char*)location;
            tempRoot["organization"] = (char*)organization;
            tempRoot["measurename"] = (char*)tempMeasureName;
            tempRoot["unitofmeasure"] = (char*)tempUnitOfMeasure;
            float Temp = 0;

            hmdtRoot["guid"] = (char*)deviceId;
            hmdtRoot["displayname"] = (char*)deviceId;
            hmdtRoot["location"] = (char*)location;
            hmdtRoot["organization"] = (char*)organization;
            hmdtRoot["measurename"] = (char*)hmdtMeasureName;
            hmdtRoot["unitofmeasure"] = (char*)hmdtUnitOfMeasure;
            float Hmdt = 0;

            char dataBuffer[JSON_BUFFER_SIZE];
            unsigned char* dataUCBuffer;

            // Variables for time stamp creation
            time_t epochTime;
            struct tm* currentTime;
            char timeISOString[25];
            
            // Send new data point regularly.
            while (1)
            {
                // Get sensor data
                Temp = dht.readTemperature();
                Hmdt = dht.readHumidity();

                // Get current time stamp
                epochTime = time(NULL);
                currentTime = localtime(&epochTime);
                sprintf(timeISOString, "%d-%02d-%02dT%02d:%02d:%02d.000Z",currentTime->tm_year+1900, currentTime->tm_mon+1, currentTime->tm_mday, currentTime->tm_hour, currentTime->tm_min, currentTime->tm_sec);

                // Prepare JSON message
                tempRoot["timecreated"] = (char*)timeISOString;
                tempRoot["value"] = Temp;
                hmdtRoot["timecreated"] = (char*)timeISOString;
                hmdtRoot["value"] = Hmdt;

                // Create JSON buffer
                data.printTo(dataBuffer, sizeof(dataBuffer));
                
                // We need to convert the char* buffer into an unsigned char* one, as we need UTF8 encoded data sent to Azure
                dataUCBuffer = (unsigned char*) &dataBuffer[0];
                LogInfo("Sending message: %s\r\n", dataUCBuffer);

                // Send JSON message to Azure IoT Hub
                sendMessage(iotHubClientHandle, dataUCBuffer, strlen(dataBuffer)*sizeof(unsigned char));
                
                IoTHubClient_LL_DoWork(iotHubClientHandle);
                ThreadAPI_Sleep(500);
            }
        }
        IoTHubClient_LL_Destroy(iotHubClientHandle);
    }
}
