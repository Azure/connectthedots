
// Copyright(c) 2013 Sandeep Mistry
// from project https://github.com/sandeepmistry/node-sensortag
//
// The MIT License(MIT)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files(the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

var NobleDevice = require('noble-device');

var Common = require('./common');

var ACCELEROMETER_UUID                      = 'f000aa1004514000b000000000000000';
var MAGNETOMETER_UUID                       = 'f000aa3004514000b000000000000000';
var GYROSCOPE_UUID                          = 'f000aa5004514000b000000000000000';
var BAROMETRIC_PRESSURE_UUID                = 'f000aa4004514000b000000000000000';
var TEST_UUID                               = 'f000aa6004514000b000000000000000';
var OAD_UUID                                = 'f000ffc004514000b000000000000000';

var ACCELEROMETER_CONFIG_UUID               = 'f000aa1204514000b000000000000000';
var ACCELEROMETER_DATA_UUID                 = 'f000aa1104514000b000000000000000';
var ACCELEROMETER_PERIOD_UUID               = 'f000aa1304514000b000000000000000';

var MAGNETOMETER_CONFIG_UUID                = 'f000aa3204514000b000000000000000';
var MAGNETOMETER_DATA_UUID                  = 'f000aa3104514000b000000000000000';
var MAGNETOMETER_PERIOD_UUID                = 'f000aa3304514000b000000000000000';

var BAROMETRIC_PRESSURE_CONFIG_UUID         = 'f000aa4204514000b000000000000000';
var BAROMETRIC_PRESSURE_CALIBRATION_UUID    = 'f000aa4304514000b000000000000000';

var GYROSCOPE_CONFIG_UUID                   = 'f000aa5204514000b000000000000000';
var GYROSCOPE_DATA_UUID                     = 'f000aa5104514000b000000000000000';
var GYROSCOPE_PERIOD_UUID                   = 'f000aa5304514000b000000000000000';

var TEST_DATA_UUID                          = 'f000aa6104514000b000000000000000';
var TEST_CONFIGURATION_UUID                 = 'f000aa6204514000b000000000000000';

var CC2540SensorTag = function(peripheral) {
  NobleDevice.call(this, peripheral);
  Common.call(this);

  this.type = 'cc2540';

  this.onAccelerometerChangeBinded      = this.onAccelerometerChange.bind(this);
  this.onMagnetometerChangeBinded       = this.onMagnetometerChange.bind(this);
  this.onGyroscopeChangeBinded          = this.onGyroscopeChange.bind(this);
};

CC2540SensorTag.is = function(peripheral) {
  var localName = peripheral.advertisement.localName;

  return (localName === 'SensorTag') ||
          (localName === 'TI BLE Sensor Tag');
};

NobleDevice.Util.inherits(CC2540SensorTag, NobleDevice);
NobleDevice.Util.mixin(CC2540SensorTag, NobleDevice.DeviceInformationService);
NobleDevice.Util.mixin(CC2540SensorTag, Common);

CC2540SensorTag.prototype.convertIrTemperatureData = function(data, callback) {
  // For computation refer :  http://processors.wiki.ti.com/index.php/SensorTag_User_Guide#IR_Temperature_Sensor

  var ambientTemperature = data.readInt16LE(2) / 128.0;

  var Vobj2 = data.readInt16LE(0) * 0.00000015625;
  var Tdie2 = ambientTemperature + 273.15;
  var S0 = 5.593 * Math.pow(10, -14);
  var a1 = 1.75 * Math.pow(10 , -3);
  var a2 = -1.678 * Math.pow(10, -5);
  var b0 = -2.94 * Math.pow(10, -5);
  var b1 = -5.7 * Math.pow(10, -7);
  var b2 = 4.63 * Math.pow(10, -9);
  var c2 = 13.4;
  var Tref = 298.15;
  var S = S0 * (1 + a1 * (Tdie2 - Tref) + a2 * Math.pow((Tdie2 - Tref), 2));
  var Vos = b0 + b1 * (Tdie2 - Tref) + b2 * Math.pow((Tdie2 - Tref), 2);
  var fObj = (Vobj2 - Vos) + c2 * Math.pow((Vobj2 - Vos), 2);
  var objectTemperature = Math.pow(Math.pow(Tdie2, 4) + (fObj/S), 0.25);
  objectTemperature = (objectTemperature - 273.15);

  callback(objectTemperature, ambientTemperature);
};

CC2540SensorTag.prototype.convertHumidityData = function(data, callback) {
  var temperature = -46.85 + 175.72 / 65536.0 * data.readUInt16LE(0);
  var humidity = -6.0 + 125.0 / 65536.0 * (data.readUInt16LE(2) & ~0x0003);

  callback(temperature, humidity);
};

CC2540SensorTag.prototype.enableBarometricPressure = function(callback) {
  this.writeUInt8Characteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_CONFIG_UUID, 0x02, function(error) {
    if (error) {
      return callback(error);
    }

    this.readDataCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_CALIBRATION_UUID, function(error, data) {
      if (error) {
        return callback(error);
      }

      this._barometricPressureCalibrationData = data;

      this.enableConfigCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_CONFIG_UUID, callback);
    }.bind(this));
  }.bind(this));
};

CC2540SensorTag.prototype.convertBarometricPressureData = function(data, callback) {

  // For computation refer :  http://processors.wiki.ti.com/index.php/SensorTag_User_Guide#Barometric_Pressure_Sensor_2
  var temp;     // Temperature raw value from sensor
  var pressure; // Pressure raw value from sensor
  var S;        // Interim value in calculation
  var O;        // Interim value in calculation
  var p_a;      // Pressure actual value in unit Pascal.
  var Pa;       // Computed value of the function

  var c0 = this._barometricPressureCalibrationData.readUInt16LE(0);
  var c1 = this._barometricPressureCalibrationData.readUInt16LE(2);
  var c2 = this._barometricPressureCalibrationData.readUInt16LE(4);
  var c3 = this._barometricPressureCalibrationData.readUInt16LE(6);

  var c4 = this._barometricPressureCalibrationData.readInt16LE(8);
  var c5 = this._barometricPressureCalibrationData.readInt16LE(10);
  var c6 = this._barometricPressureCalibrationData.readInt16LE(12);
  var c7 = this._barometricPressureCalibrationData.readInt16LE(14);

  temp = data.readInt16LE(0);
  pressure = data.readUInt16LE(2);

  S = c2 + ((c3 * temp)/ 131072.0) + ((c4 * (temp * temp)) / 17179869184.0);
  O = (c5 * 16384.0) + (((c6 * temp) / 8)) + ((c7 * (temp * temp)) / 524288.0);
  Pa = (((S * pressure) + O) / 16384.0);

  Pa /= 100.0;

  callback(Pa);
};

CC2540SensorTag.prototype.enableAccelerometer = function(callback) {
  this.enableConfigCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_CONFIG_UUID, callback);
};

CC2540SensorTag.prototype.disableAccelerometer = function(callback) {
  this.disableConfigCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_CONFIG_UUID, callback);
};

CC2540SensorTag.prototype.readAccelerometer  = function(callback) {
  this.readDataCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertAccelerometerData(data, function(x, y, z) {
      callback(null, x, y, z);
    }.bind(this));
  }.bind(this));
};

CC2540SensorTag.prototype.onAccelerometerChange = function(data) {
  this.convertAccelerometerData(data, function(x, y, z) {
    this.emit('accelerometerChange', x, y, z);
  }.bind(this));
};

CC2540SensorTag.prototype.convertAccelerometerData = function(data, callback) {
  var x = data.readInt8(0) / 16.0;
  var y = data.readInt8(1) / 16.0;
  var z = data.readInt8(2) / 16.0;

  callback(x, y, z);
};

CC2540SensorTag.prototype.notifyAccelerometer = function(callback) {
  this.notifyCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_DATA_UUID, true, this.onAccelerometerChangeBinded, callback);
};

CC2540SensorTag.prototype.unnotifyAccelerometer = function(callback) {
  this.notifyCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_DATA_UUID, false, this.onAccelerometerChangeBinded, callback);
};

CC2540SensorTag.prototype.setAccelerometerPeriod = function(period, callback) {
  this.writePeriodCharacteristic(ACCELEROMETER_UUID, ACCELEROMETER_PERIOD_UUID, period, callback);
};

CC2540SensorTag.prototype.enableMagnetometer = function(callback) {
  this.enableConfigCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_CONFIG_UUID, callback);
};

CC2540SensorTag.prototype.disableMagnetometer = function(callback) {
  this.disableConfigCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_CONFIG_UUID, callback);
};

CC2540SensorTag.prototype.readMagnetometer = function(callback) {
  this.readDataCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertMagnetometerData(data, function(x, y, z) {
      callback(null, x, y, z);
    }.bind(this));
  }.bind(this));
};

CC2540SensorTag.prototype.onMagnetometerChange = function(data) {
  this.convertMagnetometerData(data, function(x, y, z) {
    this.emit('magnetometerChange', x, y, z);
  }.bind(this));
};

CC2540SensorTag.prototype.convertMagnetometerData = function(data, callback) {
  var x = data.readInt16LE(0) * 2000.0 / 65536.0;
  var y = data.readInt16LE(2) * 2000.0 / 65536.0;
  var z = data.readInt16LE(4) * 2000.0 / 65536.0;

  callback(x, y, z);
};

CC2540SensorTag.prototype.notifyMagnetometer = function(callback) {
  this.notifyCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_DATA_UUID, true, this.onMagnetometerChangeBinded, callback);
};

CC2540SensorTag.prototype.unnotifyMagnetometer = function(callback) {
  this.notifyCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_DATA_UUID, false, this.onMagnetometerChangeBinded, callback);
};

CC2540SensorTag.prototype.setMagnetometerPeriod = function(period, callback) {
  this.writePeriodCharacteristic(MAGNETOMETER_UUID, MAGNETOMETER_PERIOD_UUID, period, callback);
};

CC2540SensorTag.prototype.setGyroscopePeriod = function(period, callback) {
  this.writePeriodCharacteristic(GYROSCOPE_UUID, GYROSCOPE_PERIOD_UUID, period, callback);
};

CC2540SensorTag.prototype.enableGyroscope = function(callback) {
  this.writeUInt8Characteristic(GYROSCOPE_UUID, GYROSCOPE_CONFIG_UUID, 0x07, callback);
};

CC2540SensorTag.prototype.disableGyroscope = function(callback) {
  this.disableConfigCharacteristic(GYROSCOPE_UUID, GYROSCOPE_CONFIG_UUID, callback);
};

CC2540SensorTag.prototype.readGyroscope = function(callback) {
  this.readDataCharacteristic(GYROSCOPE_UUID, GYROSCOPE_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertGyroscopeData(data, function(x, y, z) {
      callback(null, x, y, z);
    }.bind(this));
  }.bind(this));
};

CC2540SensorTag.prototype.onGyroscopeChange = function(data) {
  this.convertGyroscopeData(data, function(x, y, z) {
    this.emit('gyroscopeChange', x, y, z);
  }.bind(this));
};

CC2540SensorTag.prototype.convertGyroscopeData = function(data, callback) {
  var x = data.readInt16LE(0) * (500.0 / 65536.0) * -1;
  var y = data.readInt16LE(2) * (500.0 / 65536.0);
  var z = data.readInt16LE(4) * (500.0 / 65536.0);

  callback(x, y, z);
};

CC2540SensorTag.prototype.notifyGyroscope = function(callback) {
  this.notifyCharacteristic(GYROSCOPE_UUID, GYROSCOPE_DATA_UUID, true, this.onGyroscopeChangeBinded, callback);
};

CC2540SensorTag.prototype.unnotifyGyroscope = function(callback) {
  this.notifyCharacteristic(GYROSCOPE_UUID, GYROSCOPE_DATA_UUID, false, this.onGyroscopeChangeBinded, callback);
};

CC2540SensorTag.prototype.readTestData = function(callback) {
  this.readUInt16LECharacteristic(TEST_UUID, TEST_DATA_UUID, callback);
};

CC2540SensorTag.prototype.readTestConfiguration = function(callback) {
  this.readUInt8Characteristic(TEST_UUID, TEST_CONFIGURATION_UUID, callback);
};

module.exports = CC2540SensorTag;
