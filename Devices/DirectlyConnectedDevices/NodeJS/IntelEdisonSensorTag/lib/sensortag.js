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

var CC2540SensorTag = require('./cc2540');
var CC2650SensorTag = require('./cc2650');

var SensorTag = function() {
};

SensorTag.discoverAll = function(onDiscover) {
  CC2540SensorTag.discoverAll(onDiscover);
  CC2650SensorTag.discoverAll(onDiscover);
};

SensorTag.stopDiscoverAll = function(onDiscover) {
  CC2540SensorTag.stopDiscoverAll(onDiscover);
  CC2650SensorTag.stopDiscoverAll(onDiscover);
};

SensorTag.discover = function(callback) {
  var onDiscover = function(sensorTag) {
    SensorTag.stopDiscoverAll(onDiscover);

    callback(sensorTag);
  };

  SensorTag.discoverAll(onDiscover);
};

SensorTag.discoverByUuid = function(uuid, callback) {
  var onDiscoverByUuid = function(sensorTag) {
    if (sensorTag.uuid === uuid) {
      SensorTag.stopDiscoverAll(onDiscoverByUuid);

      callback(sensorTag);
    }
  };

  SensorTag.discoverAll(onDiscoverByUuid);
};

SensorTag.CC2540 = CC2540SensorTag;
SensorTag.CC2650 = CC2650SensorTag;

module.exports = SensorTag;
