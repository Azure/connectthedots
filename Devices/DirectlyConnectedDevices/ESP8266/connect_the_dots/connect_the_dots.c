// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include "AzureIoTHub.h"
#include "sdk/schemaserializer.h"
#include "dht22.h"


// Find under Microsoft Azure IoT Suite -> DEVICES -> <your device> -> Device Details and Authentication Keys
static const char* deviceId = "[device-id]";
static const char* connectionString = "[Device Connection String]";

static const char* organization = "My Organization";
static const char* location = "My Location";
static const char* tempmeasurename = "Temperature";
static const char* tempunitofmeasure = "F";
static const char* hmdtmeasurename = "Humidity";
static const char* hmdtunitofmeasure = "%";

// Define the Model
BEGIN_NAMESPACE(Contoso);

DECLARE_MODEL(CTDDevice,

    WITH_DATA(ascii_char_ptr, guid),
    WITH_DATA(ascii_char_ptr, organization),
    WITH_DATA(ascii_char_ptr, displayname),
    WITH_DATA(ascii_char_ptr, location),
    WITH_DATA(ascii_char_ptr, measurename),
    WITH_DATA(ascii_char_ptr, unitofmeasure),
    WITH_DATA(float, value)
);

END_NAMESPACE(Contoso);

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
    free((void*)buffer);
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
        result = EXECUTE_COMMAND_ERROR;
    }
    else
    {
        /*buffer is not zero terminated*/
        char* temp = malloc(size + 1);
        if (temp == NULL)
        {
            LogInfo("failed to malloc\r\n");
            result = EXECUTE_COMMAND_ERROR;
        }
        else
        {
            EXECUTE_COMMAND_RESULT executeCommandResult;

            memcpy(temp, buffer, size);
            temp[size] = '\0';
            executeCommandResult = EXECUTE_COMMAND(userContextCallback, temp);
            result =
                (executeCommandResult == EXECUTE_COMMAND_ERROR) ? IOTHUBMESSAGE_ABANDONED :
                (executeCommandResult == EXECUTE_COMMAND_SUCCESS) ? IOTHUBMESSAGE_ACCEPTED :
                IOTHUBMESSAGE_REJECTED;
            free(temp);
        }
    }
    return result;
}

void connect_the_dots_run(void)
{
        initDht();

        srand((unsigned int)time(NULL));
        if (serializer_init(NULL) != SERIALIZER_OK)
        {
            LogInfo("Failed on serializer_init\r\n");
        }
        else
        {
            IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle = IoTHubClient_LL_CreateFromConnectionString(connectionString, HTTP_Protocol);
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

                CTDDevice* ctdDevice = CREATE_MODEL_INSTANCE(Contoso, CTDDevice);
                if (ctdDevice == NULL)
                {
                    LogInfo("Failed on CREATE_MODEL_INSTANCE\r\n");
                }
                else
                {
                    STRING_HANDLE commandsMetadata;

                    if (IoTHubClient_LL_SetMessageCallback(iotHubClientHandle, IoTHubMessage, ctdDevice) != IOTHUB_CLIENT_OK)
                    {
                        LogInfo("unable to IoTHubClient_SetMessageCallback\r\n");
                    }
                    else
                    {
                        ctdDevice->guid = (char*)deviceId;
                        ctdDevice->location = (char*)location;
                        ctdDevice->organization = (char*)organization;

                        float Temp;
                        float Hmdt;
                        unsigned char*tempBuffer;
                        size_t tempBufferSize;
                        unsigned char*hmdtBuffer;
                        size_t hmdtBufferSize;

                        /* Send new data point every second. */
                        while (1)
                        {
                            getNextSample(&Temp, &Hmdt);

                            // Send temperature
                            ctdDevice->measurename = (char*)tempmeasurename;
                            ctdDevice->unitofmeasure = (char*)tempunitofmeasure;
                            ctdDevice->value = Temp;

                            LogInfo("Sending Temperature = %d\r\n", Temp);

                            if (SERIALIZE(&tempBuffer, &tempBufferSize, ctdDevice->guid, ctdDevice->organization, ctdDevice->displayname, ctdDevice->location, ctdDevice->measurename, ctdDevice->unitofmeasure, ctdDevice->value) != IOT_AGENT_OK)
                            {
                                LogInfo("Failed sending sensor value\r\n");
                            }
                            else
                            {
                                sendMessage(iotHubClientHandle, tempBuffer, tempBufferSize);
                            }

                            // Send humidity
                            ctdDevice->measurename = (char*)hmdtmeasurename;
                            ctdDevice->unitofmeasure = (char*)hmdtunitofmeasure;
                            ctdDevice->value = Hmdt;

                            LogInfo("Sending Huimidity = %d\r\n", Hmdt);

                            if (SERIALIZE(&hmdtBuffer, &hmdtBufferSize, ctdDevice->guid, ctdDevice->organization, ctdDevice->displayname, ctdDevice->location, ctdDevice->measurename, ctdDevice->unitofmeasure, ctdDevice->value) != IOT_AGENT_OK)
                            {
                                LogInfo("Failed sending sensor value\r\n");
                            }
                            else
                            {
                                sendMessage(iotHubClientHandle, hmdtBuffer, hmdtBufferSize);
                            }

                            IoTHubClient_LL_DoWork(iotHubClientHandle);
                            ThreadAPI_Sleep(1000);
                        }
                    }

                    DESTROY_MODEL_INSTANCE(ctdDevice);
                }
                IoTHubClient_LL_Destroy(iotHubClientHandle);
            }
            serializer_deinit();

    }
}
