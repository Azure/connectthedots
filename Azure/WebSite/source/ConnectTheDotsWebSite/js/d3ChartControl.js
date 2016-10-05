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

function d3ChartControl(containerId) {
    var self = this;
    // call base class contructor
    baseClass.call(self);
    // create ul
    self._containerId = containerId;
    self._ulOptions = {};

    this._onEventObjectHandler = function (event) {
        self._onNewDataHandler.call(self, event);
    };

    return self;
}

d3ChartControl.prototype = {
    constructor: d3ChartControl,
    destroy: function () {
        var self = this;
        if (self._dataSource) {
            self._dataSource.removeEventListener('eventObject', this._onEventObjectHandler);
        }
    },
    /*
        params = 
        {
            guid: 'someGUID',
            title: 'someTitle',
            selected: false,        // default true
            allOption: true         // if true, guid = 0 and it's main switcher for all
        }
    */
    setOption: function (params) {
        var self = this;

        var guid = params.guid;
        if (guid == undefined) return;

        // check if exists or create new
        if (!self._ulOptions.hasOwnProperty(params.guid)) {
            self._ulOptions[guid] = {};
            self._ulOptions[guid].li = $('<li><div style="display:inline-block">' + params.title + "</div></li>").appendTo("#" + self._containerId);
            // set selected class
            if (!params.hasOwnProperty('selected') || params.selected == true) {
                self._ulOptions[guid].li.addClass('selected');
            }
            self._ulOptions[guid].state = self._ulOptions[guid].li.hasClass('selected');
            // add click handler
            self._ulOptions[guid].li.on('click', function (evt) {
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                    self.raiseEvent('removeGuid', guid);
                } else {
                    $(this).addClass('selected');
                    self.raiseEvent('addGuid', guid);
                }
                self._ulOptions[guid].state = self._ulOptions[guid].li.hasClass('selected');
            });
            
            self._ulOptions[guid].li
                .append('<div class="sensorTip">' + 'Location: ' + params.location + '</div>');

            self._ulOptions[guid].li.each(function () {
                $(this).data('sensorTip', $(this).find('.sensorTip'));
                $(this).data('sensorTip').hide();
            });
            self._ulOptions[guid].li.each(function() {
                $(this).on('mouseover', function () {
                    $(this).data('sensorTip').show();
                });
            });
            self._ulOptions[guid].li.each(function () {
                $(this).on('mouseout', function () {
                    $(this).data('sensorTip').hide();
                });
            });
        }

        return self;
    },
    attachToDataSource: function (dataSource) {
        var self = this;
        // remebmer data source
        self._dataSource = dataSource;

        // register events handler
        dataSource.addEventListener('eventObject', this._onEventObjectHandler);

        return self;
    },
    // private members
    _onNewDataHandler: function (eventObject) {
        var self = this;
        var evt = eventObject.owner;

        // check GUID
        if (!evt.guid || self._ulOptions.hasOwnProperty(evt.guid)) return;

        // add new option
        self.setOption({
            guid: evt.guid,
            title: evt.displayname,
            location: evt.location ? evt.location : "Unknown"
        });
    },
    checkGUID : function(guid) {
        var self = this;
        return (self._ulOptions.hasOwnProperty(guid) && self._ulOptions[guid].state);
    }
};

extendClass(d3ChartControl, baseClass);