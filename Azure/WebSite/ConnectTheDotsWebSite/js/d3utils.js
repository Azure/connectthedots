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
function extendClass(Child, Parent) {
    var F = function () {
    };
    F.prototype = Parent.prototype;
    var f = new F();

    for (var prop in Child.prototype)
        f[prop] = Child.prototype[prop];
    Child.prototype = f;
    Child.prototype.superclass = Parent.prototype;
}

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

/**
 * Base class initialization
 */
function baseClass() {
    // set protected properties
    this._eventsListeners = {};
}

baseClass.prototype = {
    constructor: baseClass,
    addEventListeners: function (callbacks) {
        var self = this;
        for (var id in callbacks) {
            self.addEventListener(id, callbacks[id]);
        }
        return self;
    },
    addEventListener: function (eventName, callback) {
        var self = this;

        if (!self._eventsListeners.hasOwnProperty(eventName)) {
            self._eventsListeners[eventName] = [];
        }
        self._eventsListeners[eventName].push(callback);

        return self;
    },

    removeEventListener: function (eventName, callback) {
        var self = this;

        if (!self._eventsListeners.hasOwnProperty(eventName) || !self._eventsListeners[eventName].length)
            return;
        for (var i = this._eventsListeners[eventName].length; i > 0; --i)
            if (self._eventsListeners[eventName][i - 1] === callback) {
                self._eventsListeners[eventName].splice(i - 1, 1);
                break;
            }

        return self;
    },

    raiseEvent: function (eventName, owner) {
        if (!this._eventsListeners.hasOwnProperty(eventName) || !this._eventsListeners[eventName].length)
            return;
        var context = {
            name: eventName,
            source: this,
            owner: owner,
            handled: false
        };
        var evFuncs = this._eventsListeners[eventName];
        for (var i = evFuncs.length; i > 0; --i) {
            evFuncs[i - 1].call(this, context);
            // if handled - stop event handling
            if (context.handled)
                break;
        }
    },
};
