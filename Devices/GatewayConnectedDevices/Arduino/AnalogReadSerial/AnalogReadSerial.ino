/*
  Modifications by Microsoft Open Technologies, Inc

  AnalogReadSerial
  Reads an analog input on pin 0, prints the result to the serial monitor.
  Attach the center pin of a potentiometer to pin A0, and the outside pins to +5V and ground.

 This example code is in the public domain.
 */

// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600);
  SerialUSB.begin(9600);
}

char buffer[100];

// the loop routine runs over and over again forever:
void loop() {
  // read the input on analog pin 0:
  int sensorValue = analogRead(A0);
  
  // Turn into JSON format
  sprintf(buffer, "{ \"%s\" : %f }", "temp", (float) sensorValue);
  
  // print out the value you read:
  SerialUSB.println(buffer);
  Serial.println(buffer);
  delay(500);        // delay in between reads
}
