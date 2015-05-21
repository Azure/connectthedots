#include <SoftwareSerial.h>

SoftwareSerial bt(4,5);

void setup() 
{
    Serial.begin(9600);  
    bt.begin(9600);  
    randomSeed(analogRead(A1));
}
 
void loop() 
{
    // init array with random bytes from my head
    static uint8_t bytes[2] = { 0x02, 0x1A };
    while (bt.available() > 0) 
    {
        char incomingByte = bt.read();
        Serial.println(incomingByte);
    }
    bytes[0] = random(0xFF);
    bytes[1] = random(0xFF);
    uint16_t number = bytes[0] << 8 + bytes[1];
    Serial.print("Sending to BT:");
    Serial.println(number, DEC);
    bt.write(bytes, 2);
    delay(500);
}