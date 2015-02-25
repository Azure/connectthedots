//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

// create dataFlow with
/*
flowGUID : string,
params = {
	yMin : number,
	yMax : number,
	displayName : string,
	label : string
};*/

function d3DataFlow(flowGUID, params) {
    var self = this;
    // call base class contructor
    baseClass.call(self);
    // initialize object
    self._guid = flowGUID;
    self._yMin = params ? params.yMin : undefined;
    self._yMax = params ? params.yMax : undefined;
    self._displayName = params ? params.displayName : undefined;
    self._label = params ? params.label : undefined;
    self._CONSTANTS = {
        MAX_ARRAY_SIZE: 1000
    };

    self.clearData();
    return self;
}

d3DataFlow.prototype = {
    constructor: d3DataFlow,
    attachToDataSource: function (dataSource) {
        var self = this;
        // remebmer data source
        self._dataSource = dataSource;

        // register events handler
        dataSource.addEventListener('newData', function (event) {
            self._onNewDataHandler.call(self, event);
        });

        return self;
    },
    getGUID: function () {
        return this._guid;
    },
    yMin: function (yMinNew) {
        if (yMinNew != undefined) {
            this._yMin = yMinNew;
        }
        return this._yMin;
    },
    yMax: function (yMaxNew) {
        if (yMaxNew != undefined) {
            this._yMax = yMaxNew;
        }
        return this._yMax;
    },
    displayName: function (displayNameNew) {
        if (displayNameNew != undefined) {
            this._displayName = displayNameNew;
        }
        return this._displayName;
    },
    label: function (labelNew) {
        if (labelNew != undefined) {
            this._label = labelNew;
        }
        return this._label;
    },
    yAxis: function (yAxisNew) {
        if (yAxisNew != undefined) {
            this._yAxis = yAxisNew;
        }
        return this._yAxis;
    },
    clearData: function () {
        this._data = [];
    },
    cutData: function (cutoff) {
        var len = this._data.length;
        while (this._data.length >= 1 && this._data[0].time < cutoff) {
            this._data.shift();
        }
        return len != this._data.length;
    },
    getData: function () {
        return this._data;
    },
    addNewPoint: function (obj) {
        var self = this;
        var t = new Date(obj.time);
        if (isNaN(t.getTime())) {
            return;
        }

        var pushObj = {
            data: obj.value,
            time: new Date(obj.time)
        };

        if (obj.alerttype)
            pushObj.alertData = { message: obj.message };


        self._data.push(pushObj);

        if (self._data.length >= self._CONSTANTS.MAX_ARRAY_SIZE) {
            self._data.shift();
            return;
        }
    },
    // private members
    _onNewDataHandler: function (evt) {
        var self = this;
        var object = evt.owner;
        // filter GUID
        if (object.guid != self._guid) return;

        // add to array
        self.addNewPoint(object);

        // update properties
        self._updateProperties(object);
    },
    _updateProperties: function (eventObject) {
        var self = this;

        if (eventObject.hasOwnProperty("displayname")) {
            self.displayName(eventObject.displayname);
        }

        if (eventObject.hasOwnProperty("measurename")) {
            self.label(eventObject.measurename + " (" + eventObject.unitofmeasure + ")");
        }

        self.raiseEvent('change', self);
    }
};

extendClass(d3DataFlow, baseClass);