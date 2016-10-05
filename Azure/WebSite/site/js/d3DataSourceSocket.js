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
function d3DataSourceSocket(uri, handlers) {
    var self = this;
    // call base class contructor
    baseClass.call(self);

    // initialize object
    self._websocket = new WebSocket(uri);
    self._eventHandlers = {};

    if (handlers) {
        for (id in handlers) {
            self.addEventListener(id, handlers[id]);
        }
    }

    // register handlers
    self._websocket.onopen = function () {
        self.raiseEvent.call(self, 'open');
    }

    self._websocket.onerror = function (event) {
        self.raiseEvent.call(self, 'error', event);
    }

    self._websocket.onmessage = function (event) {
        self.raiseEvent.call(self, 'message', event);
    }

    return self;
}

d3DataSourceSocket.prototype = {
    constructor: d3DataSourceSocket,
    sendMessage: function (message) {
        this._websocket.send(JSON.stringify(message));
    },
    closeSocket: function () {
        this._websocket.close();
    }
};

extendClass(d3DataSourceSocket, baseClass);