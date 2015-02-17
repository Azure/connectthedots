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

$.getScript("dataFlow.js");

// create dataFlow with
/*
containerId : string,
dataFlows = [
    dataFlowObject
]*/

function d3Chart(dataFlows) {
    var self = this;
    // initialize object
    if (dataFlows) {
        for (var id in dataFlows) {
            addFlow(dataFlows[id]);
        }
    }
}

d3Chart.prototype = {
    constructor: d3Chart,
    addFlow: function (newFlow) {
        this._flows[newFlow.getUUID()] = newFlow;
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
    ClearDataFlows: function () {
        // clear data
        for (var id in this._flows) {
            dataFlow = this._flows[id];
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
        }
    },

    registerResizeHandler: function (containerId) {
        if (!window['resizeCallback@' + containerId]) {
            window['resizeCallback@' + containerId] = true;
            $(window).bind('resize', function () {
                console.log('rezise chart: ' + containerId);
                // remove original one
                self.ClearDataFlows();
                d3.select("#" + containerId).select('svg').remove();
                // create a new one w/ correct size
                self.createChart(containerId);
            });
        }
    },
    createChart: function (containerId) {
        var self = this;

        // remember container
        self._container = $('#' + containerId);

        // recalc font size
        recalcFontSize();

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

        self._svg = d3.select("#" + containerId)
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
        registerResizeHandler(containerId);
    }
};