#include "GroveLight.h"

/**

\param[in] int Number of an input pin
*/
grove::Light::Light(int pin)
	: m_pin(pin)
{
	pinMode(m_pin, INPUT);
}

float grove::Light::inLux()
{
	float sensorValue = static_cast<float>(rawValue());
	float lightInLux = static_cast<float>(10000.0 / pow(((1023.0 - sensorValue) * 10.0 / sensorValue) * 15.0, 4.0 / 3.0));
	return (lightInLux);
}

int grove::Light::rawValue()
{
	int sensorValue = analogRead(m_pin);
	return (sensorValue);
}