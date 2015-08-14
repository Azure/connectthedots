#include "DS18B20.h"

DS18B20::DS18B20(uint16_t pin){
    ds = new OneWire(pin);
}

boolean DS18B20::search(){
    boolean isSuccess =  ds->search(addr);
    
    if(isSuccess){
        chiptype = addr[0];
        
        switch (addr[0]) {
            case 0x10:      sprintf(szName, "DS18S20");     type_s = 1;     break;
            case 0x28:      sprintf(szName, "DS18B20");     type_s = 0;     break;
            case 0x22:      sprintf(szName, "DS1822");      type_s = 0;     break;
            default:        sprintf(szName, "Unknown");     type_s = 0;     break;
        }
    }
    
    return isSuccess;
}

void DS18B20::resetsearch(){
    ds->reset_search();
}

void DS18B20::getROM(char szROM[]){
    sprintf(szROM, "%X %X %X %X %X %X %X %X", addr[0], addr[1], addr[2], addr[3], addr[4], addr[5], addr[6], addr[7]);
}

byte DS18B20::getChipType(){
    return chiptype;
}

char* DS18B20::getChipName(){
    return szName;
}

float DS18B20::getTemperature(){
    ds->reset();
    ds->select(addr);
    ds->write(0x44, 1);        // start conversion, with parasite power on at the end
     
    delay(1000);     // maybe 750ms is enough, maybe not
    // we might do a ds.depower() here, but the reset will take care of it.
 
    ds->reset();
    ds->select(addr);    
    ds->write(0xBE);         // Read Scratchpad
 
    for (int i = 0; i < 9; i++) {           // we need 9 bytes
        data[i] = ds->read();
    }
    
    // Convert the data to actual temperature
    // because the result is a 16 bit signed integer, it should
    // be stored to an "int16_t" type, which is always 16 bits
    // even when compiled on a 32 bit processor.
    int16_t raw = (data[1] << 8) | data[0];
    
    if (type_s) {
        raw = raw << 3; // 9 bit resolution default
        if (data[7] == 0x10) {
            // "count remain" gives full 12 bit resolution
            raw = (raw & 0xFFF0) + 12 - data[6];
        }
    } else {
        byte cfg = (data[4] & 0x60);
        // at lower res, the low bits are undefined, so let's zero them
        if (cfg == 0x00) raw = raw & ~7;  // 9 bit resolution, 93.75 ms
        else if (cfg == 0x20) raw = raw & ~3; // 10 bit res, 187.5 ms
        else if (cfg == 0x40) raw = raw & ~1; // 11 bit res, 375 ms
        //// default is 12 bit resolution, 750 ms conversion time
    }
    
    return (float)raw / 16.0;
}

float DS18B20::convertToFahrenheit(float celsius){
    return celsius * 1.8 + 32.0;
}
