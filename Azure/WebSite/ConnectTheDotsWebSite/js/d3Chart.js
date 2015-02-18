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
containerId : string,
dataFlows = [
    dataFlowObject
]*/

function d3Chart(containerId, dataFlows) {
    var self = this;
    // call base class contructor
    baseClass.call(self);
    // initialize object
    self._flows = {};
    self._flowsVisuals = {};
    self._containerId = containerId;
    self._CONSTANTS = {
        MS_PER_MINUTE: 60000,
        WINDOW_MINUTES: 10,
    }

    if (dataFlows) {
        for (var id in dataFlows) {
            self.addFlow(dataFlows[id]);
        }
    }
}

d3Chart.prototype = {
    constructor: d3Chart,
    addFlow: function (newFlow) {
        this._flows[newFlow.getGUID()] = newFlow;
        newFlow.attachToDataSource(this);
    },
    attachToDataSource: function (dataSource) {
        var self = this;
        // remebmer data source
        self._dataSource = dataSource;

        // register events handler
        dataSource.addEventListener('onEventObject', function (event) {
            self._onMessageHandler.call(self, event);
        });
    },
    recalcFontSize: function () {
        //Standard height, for which the body font size is correct
        var preferredHeight = 768;
        //Base font size for the page
        var fontsize = 12;
        var displayHeight = $(window).height();
        var percentage = displayHeight / preferredHeight;

        // remember font size
        this._fontSize = Math.floor(fontsize * percentage) - 1;
    },
    clearDataFlows: function () {
        // remove visual elements
        for (var id in this._flowsVisuals) {
            removeFlowVisual(id);
        }
        this._flowsVisuals = {};
        // clear data
        for (var id in this._flows) {
            // clear data set
            this._flows[id].clearData();
        }
    },
    removeFlowVisual: function (id) {
        if (!this._flowsVisuals.hasOwnProperty(id)) return;

        var dataFlow = this._flowsVisuals[id];

        if (dataFlow.path) {
            dataFlow.path.remove();
            dataFlow.path = null;
        }
        if (dataFlow.legend) {
            dataFlow.legend.remove();
            dataFlow.legend = null;
        }
        if (dataFlow.legend_r) {
            dataFlow.legend_r.remove();
            dataFlow.legend_r = null;
        }
    },
    registerResizeHandler: function (containerId) {
        if (!window['resizeCallback@' + containerId]) {
            window['resizeCallback@' + containerId] = true;
            $(window).bind('resize', function () {
                console.log('rezise chart: ' + containerId);
                // remove original one
                self.clearDataFlows();
                d3.select("#" + containerId).select('svg').remove();
                // create a new one w/ correct size
                self.createChart(containerId);
            });
        }
    },
    createChart: function () {
        var self = this;

        // remember container
        self._container = $('#' + self._containerId);

        // recalc font size
        self.recalcFontSize();

        var dataFlowsArray = [];

        self._width = self._container.width();
        self._height = self._container.height();

        // create dataFlows array
        for (var id in self._flows) {
            dataFlowsArray.push({
                id: id,
                yMin: self._flows[id].yMin(),
                yMax: self._flows[id].yMax(),
                label: self._flows[id].label(),
            });
        }

        // seed the axes with some dummy values
        self._x = d3.time.scale()
            .domain([new Date("2015-01-01T04:02:39.867841Z"), new Date("2015-01-01T04:07:39.867841Z")])
            .range([0, self._width]);

        self._y0 = d3.scale.linear()
            .range([self._height, 0]);

        if (dataFlowsArray.length > 0 && dataFlowsArray[0].yMax != null && dataFlowsArray[0].yMin != null)
            self._y0.domain([dataFlowsArray[0].yMin, dataFlowsArray[0].yMax]);

        self._y1 = d3.scale.linear()
            .range([self._height, 0]);

        if (dataFlowsArray.length > 1 && dataFlowsArray[1].yMax != null && dataFlowsArray[1].yMin != null)
            self._y1.domain([dataFlowsArray[1].yMin, dataFlowsArray[1].yMax]);

        self._svg = d3.select("#" + self._containerId)
            .append("p")
            .append("svg")
            .attr("width", self._width)
            .attr("height", self._height)
            .append("g")

        self._svg.append("g")
            .attr("class", "y0 axis")
            .call(d3.svg.axis().scale(self._y0).ticks(7).orient("left"));

        if (dataFlowsArray.length > 0 && dataFlowsArray[0].label) {
            self._svg.append("text")
                .attr("transform", "rotate(-90)")
                .attr("class", "y0 label")
                .attr("text-anchor", "middle")
                .attr("y", -50)
                .attr("x", -self._height / 2)
                .attr("dy", "1em")
                .attr("font-size", self._fontSize + "px")
                .style("fill", colors(dataFlowsArray[0].id))
                .text(dataFlowsArray[0].label);
        }

        if (dataFlowsArray.length > 1 && dataFlowsArray[1].label) {
            self._svg.append("text")
                .attr("y1", 0)

            self._svg.append("g")
                .attr("class", "y1 axis")
                .attr("transform", "translate(" + self._width + ",0)")
                .text(dataFlowsArray[1].label)
                .call(d3.svg.axis().scale(self._y1).ticks(7).orient("right"));

            self._svg.append("text")
                .attr("transform", "rotate(-90)")
                .attr("class", "y1 label")
                .attr("text-anchor", "middle")
                .attr("y", self._width + 30)
                .attr("x", -self._height / 2)
                .attr("dy", "1em")
                .attr("font-size", self._fontSize + "px")
                .style("fill", colors(dataFlowsArray[1].id))
                .text(dataFlowsArray[1].label);
        }

        self._svg.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(0," + (self._height) + ")")
            .call(d3.svg.axis().scale(self._x).orient("bottom").tickFormat(d3.time.format("%X")));

        // create tip
        self._tip = d3.tip()
            .attr('class', 'd3-tip')
            .offset([-10, 0])
            .html(function (d) {
                return "<label class='time_header'>" + d.time + "</label><label class='value_circle'>&#x25cf;</label><label class='value'>" + d.data.toFixed(2) + "</label><label class='message'>" + d.alertData.message + "</label>";
            });
        self._svg.call(self._tip);

        // register resize handler
        self.registerResizeHandler(self._containerId);
    },
    pruneOldData: function () {
        var self = this;
        var now = new Date();
        var cutoff = new Date(now - self._CONSTANTS.WINDOW_MINUTES * self._CONSTANTS.MS_PER_MINUTE)

        // cut data
        for (var id in self._flows) {
            if (self._flows[id].cutData(cutoff)) {
                self.removeFlowVisual(id);
            }
        }
    },

    updateChart: function () {

        var self = this;

        var minDate = new Date("3015-01-01T04:02:39.867841Z");
        var maxDate = new Date("1915-01-01T04:02:39.867841Z")

        var minVal = [Number.MAX_VALUE, Number.MAX_VALUE];
        var maxVal = [0, 0];

        var displayHeight = $(window).height();


        for (var id in self._flows) {
            var dataFlow = self._flows[id];
            var data = dataFlow.getData();
            if (data.length == 0 || !dataFlow.displayName()) return;

            // sort data
            data.sort(function (a, b) {
                if (a.time < b.time) return -1;
                if (a.time > b.time) return 1;
                return 0;
            });

            var y = data[0].y_axis;

            for (var j = 0; j < data.length; j++) {

                var c = data[j].data;
                var t = data[j].time;

                if (c < minVal[y]) {
                    minVal[y] = c;
                }

                if (c > maxVal[y]) {
                    maxVal[y] = c;
                }

                if (t > maxDate) {
                    maxDate = t;
                }

                if (t < minDate) {
                    minDate = t;
                }
            }
        }

        // create chart on demand
        if (self._svg == null) {
            self.createChart();
        }

        if (minVal[0] < Number.MAX_VALUE) {
            var scaleMargin = (maxVal[0] - minVal[0]) * 10 / 100;
            self._y0 = d3.scale.linear()
                .domain([minVal[0] - scaleMargin, maxVal[0] + scaleMargin])
                .range([self._height, 0]);

            var yAxisLeft = d3.svg.axis()
                .scale(self._y0)
                .orient("left")

            self._svg.selectAll("g.y0.axis")
                .call(yAxisLeft);
        }

        if (minVal[1] < Number.MAX_VALUE) {
            var scaleMargin = (maxVal[1] - minVal[1]) * 10 / 100;

            self._y1 = d3.scale.linear()
                .domain([minVal[1] - scaleMargin, maxVal[1] + scaleMargin])
                .range([self._height, 0]);

            var yAxisRight = d3.svg.axis()
                .scale(self._y1)
                .orient("right")

            self._svg.selectAll("g.y1.axis")
                .call(yAxisRight);
        }

        self._x = d3.time.scale()
            .domain([minDate, maxDate])
            .range([0, self._width]);

        var xAxis = d3.svg.axis()
            .scale(self._x)
            .tickFormat(d3.time.format("%X"))
            .orient("bottom");

        self._svg.selectAll("g.x.axis")
            .call(xAxis);

        var line = [
            d3.svg.line()
            .interpolate("monotone")
            .x(function (d) {
                return self._x(d.time);
            })
            .y(function (d) {
                return self._y0(d.data);
            }),

            d3.svg.line()
            .interpolate("monotone")
            .x(function (d) {
                return self._x(d.time);
            })
            .y(function (d) {
                return self._y1(d.data);
            })
        ];

        try {
            var pos = 0;
            for (var id in self._flows) {
                var dataGUID = id;
                var dataFlow = self._flows[id];
                var data = dataFlow.data;

                if (dataFlow.path == null) {
                    dataFlow.path = self._svg.append("g")
                        .append("path")
                        .datum(data)
                        .attr("class", "line")
                        .attr("d", line[data[0].y_axis])
                        .style("stroke", function (d) {
                            return colors(dataGUID);
                        });
                }

                dataFlow.path.datum(data)
                    .attr("d", line[data[0].y_axis]);

                // draw alert points
                for (var pnt in data) {
                    if (typeof data[pnt].alertData == 'object') {
                        if (data[pnt].alertShowed == undefined) {
                            var transferData = JSON.stringify({ alertData: data[pnt].alertData, time: data[pnt].time, data: data[pnt].data });

                            data[pnt].alertBarShowed = self._svg.append("g").append("rect")
                                .attr("class", "bar")
                                .attr("x", self._x(data[pnt].time))
                                .attr("y", 0)
                                .attr("height", self._height)
                                .attr("width", "2px")
                                .style("fill", "#e6c9cd")

                            data[pnt].alertShowed = self._svg.append("g").append("circle")
                                .attr("class", "d3-dot")
                                .attr("cx", self._x(data[pnt].time))
                                .attr("cy", data[pnt].y_axis == 0 ? self._y0(data[pnt].data) : self._y1(data[pnt].data))
                                .style("fill", "#e93541")
                                .attr("r", displayHeight / 200)
                                .on('mouseover', function () { d3.select(this).transition().attr("r", displayHeight / 130); eval("self._tip.show(" + transferData + ");") })
                                .on('mouseout', function () { d3.select(this).transition().attr("r", displayHeight / 200); self._tip.hide(); });
                        } else {
                            data[pnt].alertShowed.attr("cx", self._x(data[pnt].time))
                                .attr("cy", data[pnt].y_axis == 0 ? self._y0(data[pnt].data) : self._y1(data[pnt].data));

                            data[pnt].alertBarShowed
                                .attr("x", self._x(data[pnt].time))
                        }
                    }
                }
                if (dataFlow.legend == null) {
                    dataFlow.legend_r = self._svg.append("rect")
                        .attr("class", "legend")
                        .attr("width", 10)
                        .attr("height", 10)
                        .attr("x", self._width + 50)
                        .attr("y", 20 + (20 * pos))
                        .style("fill", colors(dataGUID))
                        .style("stroke", colors(dataGUID));

                    dataFlow.legend = self._svg.append("text")
                        .attr("x", self._width + 65)
                        .attr("y", 20 + (20 * pos) + 5)
                        .attr("class", "legend")
                        .style("fill", colors(dataGUID))
                        .text(dataFlow.displayName);
                }
                else {
                    dataFlow.legend.text(dataFlow.displayName);
                }
                pos++;
            }
        } catch (e) {
            console.log(e);
        }
    },

    // private members
    _onMessageHandler: function (eventObject) {
        var self = this;
        var evt = eventObject.owner;
        if (evt.bulkData != undefined) {
            if (evt.bulkData == true) {
                // clear all flows
                self.clearDataFlows();

                self._isBulking = true;
            } else {
                // received bulk data. update graphs
                self.pruneOldData();
                self.updateChart();

                self._isBulking = false;
            }
        } else {
            // the message is data for the charts. find chart for message
            if (evt.hasOwnProperty('GUID') && self._flows.hasOwnProperty(evt.GUID)) {
                // check event time
                var now = new Date();
                var cutoff = new Date(now - self._CONSTANTS.WINDOW_MINUTES * self._CONSTANTS.MS_PER_MINUTE)

                if (evt.time < cutoff) {
                    return;
                }

                // add event
                self.raiseEvent('onNewData', evt);
            }
        }
    }
};

extendClass(d3Chart, baseClass);