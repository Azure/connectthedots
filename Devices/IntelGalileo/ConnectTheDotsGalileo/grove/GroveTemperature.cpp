#include "GroveTemperature.h"

/**

\param[in] int Number of an input pin
*/
grove::Temperature::Temperature(int pin)
	: m_pin(pin)
{
	pinMode(m_pin, INPUT);
}

float grove::Temperature::inC()
{
	float sensorValue = static_cast<float>(rawValue());
	float resistance = static_cast<float>((1023.0 - sensorValue) * 10000.0) / sensorValue;
	float temperatureInC = static_cast<float>(1.0 / (log(resistance / 10000.0) / 3975.0 + 1.0 / 298.15) - 273.15);
	return (temperatureInC);
}

float grove::Temperature::inF()
{
	float temperatureInC = inC();
	float temperatureInF = static_cast<float>(temperatureInC * 9.0 / 5.0 + 32.0);
	return (temperatureInF);
}

int grove::Temperature::rawValue()
{
	int sensorValue = analogRead(m_pin);
	return (sensorValue);
}