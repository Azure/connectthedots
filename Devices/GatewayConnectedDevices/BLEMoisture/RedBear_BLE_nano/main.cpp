/* mbed Microcontroller Library
 * Copyright (c) 2006-2013 ARM Limited
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include "mbed.h"
#include "iBeaconService.h"

/**
 * For this demo application, populate the beacon advertisement payload
 * with 2 AD structures: FLAG and MSD (manufacturer specific data).
 *
 * Reference:
 *  Bluetooth Core Specification 4.0 (Vol. 3), Part C, Section 11, 18
 */

const float  cAdvertisingIntervalSec = 5.0;
const float  cLEDStatusDurationSec = .010;
const float  cMoistureSensorPowerDurationSec = .020; 

const char cShortDeviceName[] = "MSOT_BLE_Demo:";

BLEDevice ble;
DigitalOut led1(LED1);
DigitalOut moistureSensorPower[] = { DigitalOut(P0_9), DigitalOut(P0_10), DigitalOut(P0_11) };
AnalogIn moistureSensorInput(P0_4);

// attempt to get more current by tying 3 outputs together and setting high output drive/low disable (GPIO_PIN_CNF_DRIVE_D0H1)
void nrf_gpio_cfg__high_current_output(uint32_t pin_number)
{
    NRF_GPIO->PIN_CNF[pin_number] = (GPIO_PIN_CNF_SENSE_Disabled << GPIO_PIN_CNF_SENSE_Pos)
                                            | (GPIO_PIN_CNF_DRIVE_D0H1 << GPIO_PIN_CNF_DRIVE_Pos)
                                            | (GPIO_PIN_CNF_PULL_Disabled << GPIO_PIN_CNF_PULL_Pos)
                                            | (GPIO_PIN_CNF_INPUT_Disconnect << GPIO_PIN_CNF_INPUT_Pos)
                                            | (GPIO_PIN_CNF_DIR_Output << GPIO_PIN_CNF_DIR_Pos);
}



void MeasureMoisture()
{
    // board LED is active low
    led1 = 0; 
    wait(cLEDStatusDurationSec);
    led1 = 1; 

    // turn on moisture sensor and wait for sensor reading to stabilize
    for( int index = 0; index < 3; index ++)
    {
        moistureSensorPower[index] =  1;
    }
    wait(cMoistureSensorPowerDurationSec);
    
    // read inputs
    float moisture = moistureSensorInput;
    
    // turn off moisture sensor
    for( int index = 0; index < 3; index ++)
    {
        moistureSensorPower[index] =  0;
    }

    // ADC range of 0-1 = 0-3.3V
    float mV = moisture * 3.3 * 1000.0;                                                             

    // convert ADC value into water volume content as documented at http://manuals.decagon.com/Manuals/13508_10HS_Web.pdf
    float WVC = (2.97e-9 * pow(mV,3)) - (7.37e-6 * pow(mV, 2)) + (6.69e-3 * mV) -1.92;

    // build advertising data
    ble.clearAdvertisingPayload();
    char buffer[32];
    sprintf(buffer, "%s%0.8f", cShortDeviceName, WVC );
    ble.accumulateAdvertisingPayload(GapAdvertisingData::MANUFACTURER_SPECIFIC_DATA, (const uint8_t*)buffer, strlen(buffer));
}

void Advertise()
{
    ble.startAdvertising();
}

void periodicCallback(void)
{
    MeasureMoisture();
    Advertise();
}

int main(void)
{  
    nrf_gpio_cfg__high_current_output( P0_9 );
    nrf_gpio_cfg__high_current_output( P0_10 );
    nrf_gpio_cfg__high_current_output( P0_11 );

    Ticker ticker;
    ticker.attach(periodicCallback, cAdvertisingIntervalSec);


    ble.init();
    ble.setDeviceName((uint8_t*)cShortDeviceName);
    ble.setAdvertisingInterval(cAdvertisingIntervalSec * 1000);
    
//    AnnounceDevice();
//    Advertise();

    while(1)
    {
        ble.waitForEvent(); // allows or low power operation
    }
}
