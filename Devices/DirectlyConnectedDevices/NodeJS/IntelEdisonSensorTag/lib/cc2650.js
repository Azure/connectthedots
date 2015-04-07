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

var MPU9250_UUID                            = 'f000aa8004514000b000000000000000';
var BAROMETRIC_PRESSURE_UUID                = 'f000aa4004514000b000000000000000';
var LUXOMTER_UUID                           = 'f000aa7004514000b000000000000000';

var BAROMETRIC_PRESSURE_CONFIG_UUID         = 'f000aa4204514000b000000000000000';

var MPU9250_CONFIG_UUID                     = 'f000aa8204514000b000000000000000';
var MPU9250_DATA_UUID                       = 'f000aa8104514000b000000000000000';
var MPU9250_PERIOD_UUID                     = 'f000aa8304514000b000000000000000';

var MPU9250_GYROSCOPE_MASK                  = 0x0007;
var MPU9250_ACCELEROMETER_MASK              = 0x0038;
var MPU9250_MAGNETOMETER_MASK               = 0x0040;

var LUXOMTER_CONFIG_UUID                    = 'f000aa7204514000b000000000000000';
var LUXOMTER_DATA_UUID                      = 'f000aa7104514000b000000000000000';
var LUXOMTER_PERIOD_UUID                    = 'f000aa7304514000b000000000000000';

var IO_CONFIG_UUID                          = 'f000aa6604514000b000000000000000';
var IO_DATA_UUID                            = 'f000aa6504514000b000000000000000';

var CC2650SensorTag = function(peripheral) {
  NobleDevice.call(this, peripheral);
  Common.call(this);

  this.type = 'cc2650';
  this.mpu9250mask = 0;
  this.mpu9250notifyCount = 0;

  this.onMPU9250ChangeBinded     = this.onMPU9250Change.bind(this);
  this.onLuxometerChangeBinded   = this.onLuxometerChange.bind(this);
};

CC2650SensorTag.is = function(peripheral) {
  var localName = peripheral.advertisement.localName;

  return (localName === 'CC2650 SensorTag') ||
          (localName === 'SensorTag 2.0');
};

NobleDevice.Util.inherits(CC2650SensorTag, NobleDevice);
NobleDevice.Util.mixin(CC2650SensorTag, NobleDevice.DeviceInformationService);
NobleDevice.Util.mixin(CC2650SensorTag, Common);

CC2650SensorTag.prototype.convertIrTemperatureData = function(data, callback) {
  var ambientTemperature = data.readInt16LE(2) / 128.0;
  var objectTemperature = data.readInt16LE(0) / 128.0;

  callback(objectTemperature, ambientTemperature);
};

CC2650SensorTag.prototype.convertHumidityData = function(data, callback) {
  var temperature = -40 + ((165  * data.readUInt16LE(0)) / 65536.0);
  var humidity = data.readUInt16LE(2) * 100 / 65536.0;

  callback(temperature, humidity);
};

CC2650SensorTag.prototype.enableBarometricPressure = function(callback) {
  this.enableConfigCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_CONFIG_UUID, callback);
};

CC2650SensorTag.prototype.convertBarometricPressureData = function(data, callback) {
  var tempBMP;     // Temperature processed value from sensor
  var pressure; // Pressure processed value from sensor

  // data is returned as 16 bit single precision float, convert to float
  // no idea at moment why divide by 10000 and not 100
  var exponent;
  var mantissa;

  var flTempBMP;
  var flPressure;
  tempBMP = data.readUInt16LE(0);

  exponent = (tempBMP & 0xF000) >> 12;
  mantissa = (tempBMP & 0x0FFF);

  flTempBMP = mantissa * Math.pow(2, exponent) / 10000;

  pressure = data.readUInt16LE(2);

  exponent = (pressure & 0xF000) >> 12;
  mantissa = (pressure & 0x0FFF);
  flPressure = mantissa * Math.pow(2, exponent) / 10000;

  callback(flPressure);
};

CC2650SensorTag.prototype.setMPU9250Period = function(period, callback) {
  this.writePeriodCharacteristic(MPU9250_UUID, MPU9250_PERIOD_UUID, period, callback);
};

CC2650SensorTag.prototype.enableMPU9250 = function(mask, callback) {
  this.mpu9250mask |= mask;

  // for now, always write 0x007f, magnetometer does not seem to notify is specific mask is used
  this.writeUInt16LECharacteristic(MPU9250_UUID, MPU9250_CONFIG_UUID, 0x007f, callback);
};

CC2650SensorTag.prototype.disableMPU9250 = function(mask, callback) {
  this.mpu9250mask &= ~mask;

  if (this.mpu9250mask === 0) {
    this.writeUInt16LECharacteristic(MPU9250_UUID, MPU9250_CONFIG_UUID, 0x0000, callback);
  } else if (typeof(callback) === 'function') {
    callback();
  }
};

CC2650SensorTag.prototype.notifyMPU9250 = function(callback) {
  this.mpu9250notifyCount++;

  if (this.mpu9250notifyCount === 1) {
    this.notifyCharacteristic(MPU9250_UUID, MPU9250_DATA_UUID, true, this.onMPU9250ChangeBinded, callback);
  } else if (typeof(callback) === 'function') {
    callback();
  }
};

CC2650SensorTag.prototype.unnotifyMPU9250 = function(callback) {
  this.mpu9250notifyCount--;

  if (this.mpu9250notifyCount === 0) {
    this.notifyCharacteristic(MPU9250_UUID, MPU9250_DATA_UUID, false, this.onMPU9250ChangeBinded, callback);
  } else if (typeof(callback) === 'function') {
    callback();
  }
};

CC2650SensorTag.prototype.enableAccelerometer = function(callback) {
  this.enableMPU9250(MPU9250_ACCELEROMETER_MASK, callback);
};

CC2650SensorTag.prototype.disableAccelerometer = function(callback) {
  this.disableMPU9250(MPU9250_ACCELEROMETER_MASK, callback);
};

CC2650SensorTag.prototype.readAccelerometer  = function(callback) {
  this.readDataCharacteristic(MPU9250_UUID, MPU9250_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertMPU9250Data(data, function(x, y, z) {
      callback(null, x, y, z);
    }.bind(this));
  }.bind(this));
};

CC2650SensorTag.prototype.onMPU9250Change = function(data) {
  this.convertMPU9250Data(data, function(x, y, z, xG, yG, zG, xM, yM, zM) {
    if (this.mpu9250mask & MPU9250_ACCELEROMETER_MASK) {
      this.emit('accelerometerChange', x, y, z);
    }

    if (this.mpu9250mask & MPU9250_GYROSCOPE_MASK) {
      this.emit('gyroscopeChange', xG, yG, zG);
    }

    if (this.mpu9250mask & MPU9250_MAGNETOMETER_MASK) {
      this.emit('magnetometerChange', xM, yM, zM);
    }
  }.bind(this));
};

CC2650SensorTag.prototype.convertMPU9250Data = function(data, callback) {
  // 250 deg/s range
  var xG = data.readInt16LE(0) * (500.0 / 65536.0);
  var yG = data.readInt16LE(2) * (500.0 / 65536.0);
  var zG = data.readInt16LE(4) * (500.0 / 65536.0);

  // we specify 2G range in setup
  var x = data.readInt16LE(6) * 2.0 / 32768.0;
  var y = data.readInt16LE(8) * 2.0 / 32768.0;
  var z = data.readInt16LE(10) * 2.0 / 32768.0;

  // magnetometer (page 50 of http://www.invensense.com/mems/gyro/documents/RM-MPU-9250A-00.pdf)
  var xM = data.readInt16LE(12) * 4912.0 / 32760.0;
  var yM = data.readInt16LE(14) * 4912.0 / 32760.0;
  var zM = data.readInt16LE(16) * 4912.0 / 32760.0;

  callback(x, y, z, xG, yG, zG, xM, yM, zM);
};

CC2650SensorTag.prototype.notifyAccelerometer = function(callback) {
  this.notifyMPU9250(callback);
};

CC2650SensorTag.prototype.unnotifyAccelerometer = function(callback) {
  this.unnotifyMPU9250(callback);
};

CC2650SensorTag.prototype.setAccelerometerPeriod = function(period, callback) {
  this.setMPU9250Period(period, callback);
};

CC2650SensorTag.prototype.enableMagnetometer = function(callback) {
  this.enableMPU9250(MPU9250_MAGNETOMETER_MASK, callback);
};

CC2650SensorTag.prototype.disableMagnetometer = function(callback) {
  this.disableMPU9250(MPU9250_MAGNETOMETER_MASK, callback);
};

CC2650SensorTag.prototype.readMagnetometer = function(callback) {
  this.readDataCharacteristic(MPU9250_UUID, MPU9250_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertMPU9250Data(data, function(x, y, z, xG, yG, zG, xM, yM, zM) {
      callback(null, xM, yM, zM);
    }.bind(this));
  }.bind(this));
};

CC2650SensorTag.prototype.notifyMagnetometer = function(callback) {
  this.notifyMPU9250(callback);
};

CC2650SensorTag.prototype.unnotifyMagnetometer = function(callback) {
  this.unnotifyMPU9250(callback);
};

CC2650SensorTag.prototype.setMagnetometerPeriod = function(period, callback) {
  this.setMPU9250Period(period, callback);
};

CC2650SensorTag.prototype.setGyroscopePeriod = function(period, callback) {
  this.setMPU9250Period(period, callback);
};

CC2650SensorTag.prototype.enableGyroscope = function(callback) {
  this.enableMPU9250(MPU9250_GYROSCOPE_MASK, callback);
};

CC2650SensorTag.prototype.disableGyroscope = function(callback) {
  this.disableMPU9250(MPU9250_GYROSCOPE_MASK, callback);
};

CC2650SensorTag.prototype.readGyroscope = function(callback) {
  this.readDataCharacteristic(MPU9250_UUID, MPU9250_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertMPU9250Data(data, function(x, y, z, xG, yG, zG) {
      callback(null, xG, yG, zG);
    }.bind(this));
  }.bind(this));
};

CC2650SensorTag.prototype.notifyGyroscope = function(callback) {
  this.notifyMPU9250(callback);
};

CC2650SensorTag.prototype.unnotifyGyroscope = function(callback) {
  this.unnotifyMPU9250(callback);
};

CC2650SensorTag.prototype.enableLuxometer = function(callback) {
  this.enableConfigCharacteristic(LUXOMTER_UUID, LUXOMTER_CONFIG_UUID, callback);
};

CC2650SensorTag.prototype.disableLuxometer = function(callback) {
  this.disableConfigCharacteristic(LUXOMTER_UUID, LUXOMTER_CONFIG_UUID, callback);
};

CC2650SensorTag.prototype.readLuxometer = function(callback) {
  this.readDataCharacteristic(LUXOMTER_UUID, LUXOMTER_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertLuxometerData(data, function(lux) {
      callback(null, lux);
    }.bind(this));
  }.bind(this));
 };

CC2650SensorTag.prototype.onLuxometerChange = function(data) {
  this.convertLuxometerData(data, function(lux) {
    this.emit('luxometerChange', lux);
  }.bind(this));
};

CC2650SensorTag.prototype.convertLuxometerData = function(data, callback) {
  var lux = data.readUInt16LE(0) /100;

  callback(lux);
};

CC2650SensorTag.prototype.notifyLuxometer = function(callback) {
  this.notifyCharacteristic(LUXOMTER_UUID, LUXOMTER_DATA_UUID, true, this.onLuxometerChangeBinded, callback);
};

CC2650SensorTag.prototype.unnotifyLuxometer = function(callback) {
  this.notifyCharacteristic(LUXOMTER_UUID, LUXOMTER_DATA_UUID, false, this.onLuxometerChangeBinded, callback);
};

CC2650SensorTag.prototype.setLuxometerPeriod = function(period, callback) {
  this.writePeriodCharacteristic(LUXOMTER_UUID, LUXOMTER_PERIOD_UUID, period, callback);
};

module.exports = CC2650SensorTag;
