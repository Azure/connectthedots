#pragma once

#ifndef _GROVE_TEMP_H_
#define _GROVE_TEMP_H_

#include "arduino.h"

namespace grove 
{
	class Temperature
	{
	public:
		/// setup pin for analog input
		Temperature(int pin);
		virtual ~Temperature()
		{
		};

		/// Method to get temperature value in Celsius
		float inC();

		/// Method to get temperature value in Fahrenheit
		float inF();

		/// Method to get raw value in range between 0 and 1023
		int rawValue();
	private:
		int m_pin;
	};
};

#endif // _GROVE_TEMP_H_