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

var tempChart;
var humChart;
//var lightChart;

var updateChart = true;

var websocket;

$(document).ready(function () {

// Set up jQuery DataTable to show alerts
    var table = $('#alertTable').DataTable(
    {
        "columnDefs":
        [
            {
                "targets": "timeFromDate",
                "data":
                    function (row, type, val, meta)
                    {
                        if (type === 'set') {
                            row[meta.col]= val;
                            return;
                        }
                        else if (type === 'display') {
                            return row[meta.col].toLocaleTimeString();
                        }
                        return row[meta.col];
                    }
            },
            {
                "targets": "numberFixed",
                "data":
                    function (row, type, val, meta)
                    {
                        if (type === 'set') {
                            row[meta.col]= val;
                            return;
                        }
                        else if (type === 'display') {
                            return row[meta.col].toFixed(1);
                        }
                        return row[meta.col];
                    }
            },
        ]
    });

    table.order([0, 'desc']);

    // Set up chart to show real time temperature
    tempChart = new Highcharts.Chart(
    {
        chart:
        {
            renderTo: 'tempchartContainer',
            type: 'line'
        },
        title:
        {
            text: 'Temperature'
        },
        xAxis:
        {
            labels: { format: '{value:%H:%M:%S}' }
        },
        series:
        [
            {
                name: 'Average',
                data: []
            }
        ]
    });

    // Set up chart to show real time humidity
    humChart = new Highcharts.Chart(
    {
        chart:
        {
            renderTo: 'humchartContainer',
            type: 'line'
        },
        title:
        {
            text: 'Humidity'
        },
        xAxis:
        {
            labels: { format: '{value:%H:%M:%S}' }
        }
    });

    // Set up chart to show real time light
    //lightChart = new Highcharts.Chart(
    //{
    //    chart:
    //    {
    //        renderTo: 'lightchartContainer',
    //        type: 'line'
    //    },
    //    title:
    //    {
    //        text: 'Light'
    //    },
    //    xAxis:
    //    {
    //        labels: { format: '{value:%H:%M:%S}' }
    //    }
    //});

    // Set up websocket client
    var sss = (window.location.protocol.indexOf('s') > 0 ? "s" : "");
    var uri = 'ws'+ sss +'://' + window.location.host + '/api/websocketconnect?clientId=none';
    
    websocket = new WebSocket(uri);

    $('#messages').prepend('<div> Connecting to ' + uri + '<div>');

    websocket.onopen = function ()
    {
        $('#messages').prepend('<div>Connected.</div>');
    }

    websocket.onerror = function (event)
    {
        $('#messages').prepend('<div>ERROR '+ event.error+'</div>');
    }

    // Deal with message received on WebSocket
    websocket.onmessage = function (event)
    {
        try
        {
            // Parse the JSON package
            var eventObject = JSON.parse(event.data);
        }
        catch (e)
        {
            $('#messages').prepend('<div>Malformed message: '+event.data+"</div>");
        }

        // Seems like we have valid data
        try
        {
            // If the message is an alert, we need to display it in the datatable
            if (eventObject.alerttype != null)
            {
                var table = $('#alertTable').DataTable();
                var time = new Date(eventObject.timestart);

                // Log the alert in the rawalerts div
                $('#rawalerts').prepend('<div>' + time + ': ' + eventObject.dsplalert + ' ' + eventObject.alerttype + ' ' + eventObject.message + '</div>');
                $('#rawalerts').contents().filter(':gt(20)').remove();

                // Check if we already have this one in the table already to prevent duplicates
                var indexes = table.rows().eq(0).filter(function (rowIdx) {
                    if (
                        table.cell(rowIdx, 0).data().getTime() == time.getTime()
                        && table.cell(rowIdx, 1).data() == eventObject.dsplalert
                        && table.cell(rowIdx, 2).data() == eventObject.alerttype
                    ) {
                        return true;
                    }
                    return false;
                });

                // The alert is a new one, lets display it
                if (indexes.length == 0)
                {
                    // For performance reasons, we want to limit the number of items in the table to a max of 20. 
                    // We will remove the oldest from the list
                    if (table.data().length > 19) {
                        // Search for the oldest time in the list of alerts
                        var minTime = table.data().sort(
                            function (a, b) {
                                return (a[0] > b[0]) - (a[0] < b[0])
                            }
                            )[0][0];
                        // Delete the oldest row
                        table.rows(
                            function (idx, data, node) {
                                return data[0].getTime() == minTime.getTime();
                            }
                        ).remove();
                    }

                    // Add the new alert to the table
                    var message = 'message';
                    if (eventObject.message != null) message = eventObject.message;
                    table.row.add([
                        time,
                        eventObject.dsplalert,
                        eventObject.alerttype,
                        message
                    ]).draw();
                }
            }
            else
            {
                // Message received is not an alert. let's display it in the charts
                if (eventObject.tempavg != null)
                {
                    // In the case of an average temperature ingo, we are adding the point to the tempavg series in the temp chart
                    AddPointToChartSeries(tempChart, 0, eventObject.time, eventObject.tempavg);
                }
                else if (eventObject.bulkData != null)
                {
                    // if the bulkData key is set to false in the message, we will trigger the update for the chart. 
                    if (eventObject.bulkData == true)
                    {
                        updateChart = false;
                    }
                    else
                    {
                        updateChart = true;
                        tempChart.redraw();
                        humChart.redraw();
                        //lightChart.redraw();
                    }
                }
                else
                {
                    // the message is data for the charts
                    // we make sure the dspl field is in the message, meaning the data is coming from a known device or service
                    if (eventObject.dspl != null) {
                        if (updateChart) {
                            //$('#messages').prepend('<div>' + eventObject.time + ': ' + eventObject.dspl + ' ' + eventObject.temp + ' ' + eventObject.hmdt + ' ' + eventObject.lght + '</div>');
                            //$('#messages').contents().filter(':gt(100)').remove();
                        }

                        // We first check if the device is already in the chart or if we need to add a new series to the charts
                        for (var i = 1; i < tempChart.series.length; i++) {
                            if (tempChart.series[i].name == eventObject.dspl) {
                                break;
                            }
                        }
                        if (i >= tempChart.series.length) {
                            tempChart.addSeries(
                                {
                                    name: eventObject.dspl,
                                    data: []
                                });
                            humChart.addSeries(
                                {
                                    name: eventObject.dspl,
                                    data: []
                                });
                            //lightChart.addSeries(
                            //    {
                            //        name: eventObject.dspl,
                            //        data: []
                            //    });
                        }

                        // Now we can add the point to the series
                        AddPointToChartSeries(tempChart, i, eventObject.time, eventObject.temp);
                        AddPointToChartSeries(humChart, i - 1, eventObject.time, eventObject.hmdt);
                        //AddPointToChartSeries(lightChart, i - 1, eventObject.time, eventObject.lght);
                    }
                }
            }
        }
        catch (e)
        {
            $('#messages').prepend('<div>Error processing message: ' + e.message + "</div>");
        }
    }

});

const timeToChart = 10 * 60000; // in milliseconds

function AddPointToChartSeries(chart, seriesIndex, dateString, y)
{
    var lastTimeInTicks = new Date(dateString).getTime() -new Date().getTimezoneOffset() * 60 *1000;
    var lastTimeToShowInTicks = lastTimeInTicks -timeToChart; // Only show last n minutes

    var shift = false;
    var insert = true;
    if (chart.series[seriesIndex].points.length > 0)
    {
//        if (tempChart.series[seriesIndex].points[chart.series[seriesIndex].points.length - 1].x >= lastTimeInTicks) {
        if (chart.series[seriesIndex].points[chart.series[seriesIndex].points.length - 1].x >= lastTimeInTicks) {
            // The new datapoint is older than the last one we already have for this series: ignore!
            insert = false;
        }
        else
        {
//            if (tempChart.series[seriesIndex].points[0].x < lastTimeToShowInTicks - 5 * 60000)
            if (chart.series[seriesIndex].points[0].x < lastTimeToShowInTicks - 5 * 60000)
            {
                shift = true;
            }
        }
    }
    if (insert)
    {
        //tempChart.xAxis[0].setExtremes(lastTimeToShowInTicks, null, updateChart); // Only show last n minutes
        //tempChart.series[seriesIndex].addPoint([lastTimeInTicks, y],
        //    updateChart,
        //    shift); // Shift out older value
        chart.xAxis[0].setExtremes(lastTimeToShowInTicks, null, updateChart); // Only show last n minutes
        chart.series[seriesIndex].addPoint([lastTimeInTicks, y],
            updateChart,
            shift); // Shift out older value
    }
}


function SensorSelectionChanged(dropDown)
{
    var newSensor = dropDown.value;
    if (websocket != null)
    {
        var x = { MessageType: "LiveDataSelection", DeviceName: newSensor };
        websocket.send(JSON.stringify(x));
    }
    for (var i = 1; i < tempChart.series.length; i++)
    {
        if (newSensor == "All" || tempChart.series[i].name == newSensor)
        {
            tempChart.series[i].show();
        }
        else
        {
            tempChart.series[i].hide();
        }
    }
    for (var i = 1; i < humChart.series.length; i++)
    {
        if (newSensor == "All" || humChart.series[i].name == newSensor) {
            humChart.series[i].show();
        }
        else {
            humChart.series[i].hide();
        }
    }

    //for (var i = 1; i < lightChart.series.length; i++) {
    //    if (newSensor == "All" || lightChart.series[i].name == newSensor) {
    //        lightChart.series[i].show();
    //    }
    //    else {
    //        lightChart.series[i].hide();
    //    }
    //}
}

function ShowHide(tHtml)
{
    if (tHtml)
    {
        if (tHtml.style.display == '')
            tHtml.style.display = 'none';
        else
            tHtml.style.display = '';
    }
}
