
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

// http://processors.wiki.ti.com/index.php/SensorTag_User_Guide

var IR_TEMPERATURE_UUID                     = 'f000aa0004514000b000000000000000';
var HUMIDITY_UUID                           = 'f000aa2004514000b000000000000000';
var BAROMETRIC_PRESSURE_UUID                = 'f000aa4004514000b000000000000000';
var SIMPLE_KEY_UUID                         = 'ffe0';

var IR_TEMPERATURE_CONFIG_UUID              = 'f000aa0204514000b000000000000000';
var IR_TEMPERATURE_DATA_UUID                = 'f000aa0104514000b000000000000000';
var IR_TEMPERATURE_PERIOD_UUID              = 'f000aa0304514000b000000000000000';
var IR_TEMPERATURE_PERIOD_UUID              = 'f000aa0304514000b000000000000000';

var HUMIDITY_CONFIG_UUID                    = 'f000aa2204514000b000000000000000';
var HUMIDITY_DATA_UUID                      = 'f000aa2104514000b000000000000000';
var HUMIDITY_PERIOD_UUID                    = 'f000aa2304514000b000000000000000';

var BAROMETRIC_PRESSURE_CONFIG_UUID         = 'f000aa4204514000b000000000000000';
var BAROMETRIC_PRESSURE_DATA_UUID           = 'f000aa4104514000b000000000000000';
var BAROMETRIC_PRESSURE_PERIOD_UUID         = 'f000aa4404514000b000000000000000';

var SIMPLE_KEY_DATA_UUID                    = 'ffe1';

function SensorTagCommon() {
  this.onIrTemperatureChangeBinded      = this.onIrTemperatureChange.bind(this);
  this.onHumidityChangeBinded           = this.onHumidityChange.bind(this);
  this.onBarometricPressureChangeBinded = this.onBarometricPressureChange.bind(this);
  this.onSimpleKeyChangeBinded          = this.onSimpleKeyChange.bind(this);
}

SensorTagCommon.prototype.toString = function() {
  return JSON.stringify({
    uuid: this.uuid,
    type: this.type
  });
};

SensorTagCommon.prototype.writePeriodCharacteristic = function(serviceUuid, characteristicUuid, period, callback) {
  period /= 10; // input is scaled by units of 10ms

  if (period < 10) {
    period = 10;
  } else if (period > 255) {
    period = 255;
  }

  this.writeUInt8Characteristic(serviceUuid, characteristicUuid, period, callback);
};

SensorTagCommon.prototype.enableConfigCharacteristic = function(serviceUuid, characteristicUuid, callback) {
  this.writeUInt8Characteristic(serviceUuid, characteristicUuid, 0x01, callback);
};

SensorTagCommon.prototype.disableConfigCharacteristic = function(serviceUuid, characteristicUuid, callback) {
  this.writeUInt8Characteristic(serviceUuid, characteristicUuid, 0x00, callback);
};

SensorTagCommon.prototype.setIrTemperaturePeriod = function(period, callback) {
  this.writePeriodCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_PERIOD_UUID, period, callback);
};

SensorTagCommon.prototype.enableIrTemperature = function(callback) {
  this.enableConfigCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_CONFIG_UUID, callback);
};

SensorTagCommon.prototype.disableIrTemperature = function(callback) {
  this.disableConfigCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_CONFIG_UUID, callback);
};

SensorTagCommon.prototype.readIrTemperature = function(callback) {
  this.readDataCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertIrTemperatureData(data, function(objectTemperature, ambientTemperature) {
      callback(null, objectTemperature, ambientTemperature);
    }.bind(this));
  }.bind(this));
};

SensorTagCommon.prototype.onIrTemperatureChange = function(data) {
  this.convertIrTemperatureData(data, function(objectTemperature, ambientTemperature) {
    this.emit('irTemperatureChange', objectTemperature, ambientTemperature);
  }.bind(this));
};

SensorTagCommon.prototype.notifyIrTemperature = function(callback) {
  this.notifyCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_DATA_UUID, true, this.onIrTemperatureChangeBinded, callback);
};

SensorTagCommon.prototype.unnotifyIrTemperature = function(callback) {
  this.notifyCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_DATA_UUID, false, this.onIrTemperatureChangeBinded, callback);
};

SensorTagCommon.prototype.setIrTemperaturePeriod = function(period, callback) {
  this.writePeriodCharacteristic(IR_TEMPERATURE_UUID, IR_TEMPERATURE_PERIOD_UUID, period, callback);
};

SensorTagCommon.prototype.setHumidityPeriod = function(period, callback) {
  this.writePeriodCharacteristic(HUMIDITY_UUID, HUMIDITY_PERIOD_UUID, period, callback);
};

SensorTagCommon.prototype.enableHumidity = function(callback) {
  this.enableConfigCharacteristic(HUMIDITY_UUID, HUMIDITY_CONFIG_UUID, callback);
};

SensorTagCommon.prototype.disableHumidity = function(callback) {
  this.disableConfigCharacteristic(HUMIDITY_UUID, HUMIDITY_CONFIG_UUID, callback);
};

SensorTagCommon.prototype.readHumidity = function(callback) {
  this.readDataCharacteristic(HUMIDITY_UUID, HUMIDITY_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertHumidityData(data, function(temperature, humidity) {
      callback(null, temperature, humidity);
    });
  }.bind(this));
};

SensorTagCommon.prototype.onHumidityChange = function(data) {
  this.convertHumidityData(data, function(temperature, humidity) {
    this.emit('humidityChange', temperature, humidity);
  }.bind(this));
};

SensorTagCommon.prototype.notifyHumidity = function(callback) {
  this.notifyCharacteristic(HUMIDITY_UUID, HUMIDITY_DATA_UUID, true, this.onHumidityChangeBinded, callback);
};

SensorTagCommon.prototype.unnotifyHumidity = function(callback) {
  this.notifyCharacteristic(HUMIDITY_UUID, HUMIDITY_DATA_UUID, false, this.onHumidityChangeBinded, callback);
};

SensorTagCommon.prototype.setBarometricPressurePeriod = function(period, callback) {
  this.writePeriodCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_PERIOD_UUID, period, callback);
};

SensorTagCommon.prototype.disableBarometricPressure = function(callback) {
  this.disableConfigCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_CONFIG_UUID, callback);
};

SensorTagCommon.prototype.readBarometricPressure = function(callback) {
  this.readDataCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_DATA_UUID, function(error, data) {
    if (error) {
      return callback(error);
    }

    this.convertBarometricPressureData(data, function(pressure) {
      callback(null, pressure);
    }.bind(this));
  }.bind(this));
};

SensorTagCommon.prototype.onBarometricPressureChange = function(data) {
  this.convertBarometricPressureData(data, function(pressure) {
    this.emit('barometricPressureChange', pressure);
  }.bind(this));
};

SensorTagCommon.prototype.notifyBarometricPressure = function(callback) {
  this.notifyCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_DATA_UUID, true, this.onBarometricPressureChangeBinded, callback);
};

SensorTagCommon.prototype.unnotifyBarometricPressure = function(callback) {
  this.notifyCharacteristic(BAROMETRIC_PRESSURE_UUID, BAROMETRIC_PRESSURE_DATA_UUID, false, this.onBarometricPressureChangeBinded, callback);
};

SensorTagCommon.prototype.onSimpleKeyChange = function(data) {
  this.convertSimpleKeyData(data, function(left, right) {
    this.emit('simpleKeyChange', left, right);
  }.bind(this));
};

SensorTagCommon.prototype.convertSimpleKeyData = function(data, callback) {
  var b = data.readUInt8(0);

  var left = (b & 0x2) ? true : false;
  var right = (b & 0x1) ? true : false;

  callback(left, right);
};

SensorTagCommon.prototype.notifySimpleKey = function(callback) {
  this.notifyCharacteristic(SIMPLE_KEY_UUID, SIMPLE_KEY_DATA_UUID, true, this.onSimpleKeyChangeBinded, callback);
};

SensorTagCommon.prototype.unnotifySimpleKey = function(callback) {
  this.notifyCharacteristic(SIMPLE_KEY_UUID, SIMPLE_KEY_DATA_UUID, false, this.onSimpleKeyChangeBinded, callback);
};

module.exports = SensorTagCommon;
