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

// useful function
function deepCopy(obj) {
    if (typeof obj != 'object') {
        return obj;
    }
    var copy = obj.constructor();
    for (var key in obj) {
        if (typeof obj[key] == 'object' && obj[key] != null) {
            copy[key] = this.deepCopy(obj[key]);
        } else {
            copy[key] = obj[key];
        }
    }
    return copy;
};

// create dataFlow with
/*
flowUUID : string,
params = {
	yMin : number,
	yMax : number,
	displayName : string,
	label : string
};*/

function d3DataFlow(flowUUID, params) {
    var self = this;
    // initialize object
    self._UUID = flowUUID;
    self._yMin = params ? params.yMin : undefined;
    self._yMax = params ? params.yMax : undefined;
    self._displayName = params ? params.displayName : undefined;
    self._label = params ? params.label : undefined;
}

d3DataFlow.prototype = {
    constructor: d3DataFlow,
    getUUID: function () {
        return this._UUID;
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
};