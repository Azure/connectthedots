#pragma once 

#ifndef _GROVE_LIGHT_H_
#define _GROVE_LIGHT_H_

#include "arduino.h"

namespace grove {

	class Light
	{
	public:
		/// setup pin for analog input
		Light(int pin);
		virtual ~Light()
		{
		};

		/// Method to get light level value in Lux
		float inLux();

		/// Method to get raw value in range between 0 and 1023
		int rawValue();
	private:
		int m_pin;
	};

};

#endif // _GROVE_LIGHT_H_