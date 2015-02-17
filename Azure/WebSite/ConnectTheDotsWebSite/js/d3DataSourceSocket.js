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

// create new source
function d3DataSourceSocket(uri, callbacks) {
    var self = this;
    // initialize object
    self._websocket = new WebSocket(uri);
    self._callbacks = {};

    if (callbacks) {
        for (id in callbacks) {
            self.registerCallback(id, callbacks[id]);
        }
    }

    // register handlers
    self._websocket.onopen = function () {
        self._raiseEvent.call(self, 'onopen');
    }

    self._websocket.onerror = function (event) {
        self._raiseEvent.call(self, 'onerror', event);
    }

    self._websocket.onmessage = function (event) {
        self._raiseEvent.call(self, 'onmessage', event);
    }

}

d3DataSourceSocket.prototype = {
    constructor: d3DataSourceSocket,
    raiseEvent: function (eventName, eventData) {
        // call registered callbacks
        if (this._callbacks.hasOwnProperty(eventName)) {
            var cbs = this._callbacks[eventName];
            for (var i = 0, j = cbs.length; i < j; ++i) {
                cbs[i].call(this, eventName, eventData);
            }
        }
    },
    registerCallback: function (eventName, callbackFunc) {
        if (!this._callbacks.hasOwnProperty(eventName)) {
            this._callbacks[eventName] = [];
        }
        this._callbacks[eventName].push(callbackFunc);
    },
    sendMessage: function (message) {
        websocket.send(JSON.stringify(message));
    }
};