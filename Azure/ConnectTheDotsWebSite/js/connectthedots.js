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

var myChart;
var updateChart = true;

var websocket;

$(document).ready(function () {
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

    table.order([1, 'desc']);

    myChart = new Highcharts.Chart(
    {
        chart:
        {
            renderTo: 'chartContainer',
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

    var sss = (window.location.protocol.indexOf('s')>0 ? "s" : "");
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


    websocket.onmessage = function (event)
    {
        try
        {
            var eventObject = JSON.parse(event.data);
        }
        catch (e)
        {
            $('#messages').prepend('<div>Malformed message: '+event.data+"</div>");
        }

        try
        {
            if (eventObject.alerttype != null)
            {
                var table = $('#alertTable').DataTable();

                var startTime = new Date(eventObject.timestart);
                var endTime = new Date(eventObject.timeend);

                var indexes = table.rows().eq(0).filter(function (rowIdx)
                {
                    if (
                        table.cell(rowIdx, 0).data() == eventObject.alerttype + ' ' + eventObject.dsplalert
                        && table.cell(rowIdx, 1).data().getTime() == startTime.getTime()
                        && table.cell(rowIdx, 2).data().getTime() == endTime.getTime()
                    )
                    {
                        return true;
                    }
                    return false;
                });


                if (indexes.length==0)
                {
                    if (table.data().length > 200)
                    {
                        var minTime = table.data().sort(
                            function(a,b)
                            {
                                return (a[1] > b[1]) - (a[1] < b[1])
                            }
                            )[0][1];
                        table.rows(
                            function (idx, data, node)
                            {
                                return data[1] == minTime;
                            }
                        ).remove();
                    }

                    table.row.add([
                        eventObject.alerttype+' '+ eventObject.dsplalert,
                        startTime,
                        endTime,
                        eventObject.tempmax,
                        eventObject.tempavg,
                        eventObject.tempmin,
                    ]).draw();
                }
            }
            else
            {
                if (eventObject.tempavg != null)
                {
                    AddPointToChartSeries(myChart, 0, eventObject.time, eventObject.tempavg);
                }
                else if (eventObject.bulkData != null)
                {
                    if (eventObject.bulkData == true)
                    {
                        updateChart = false;
                    }
                    else
                    {
                        updateChart = true;
                        myChart.redraw();
                    }
                }
                else
                {
                    if (updateChart)
                    {
                        $('#messages').prepend('<div>' + eventObject.time + ': ' + eventObject.dspl + ' ' + eventObject.temp + '</div>');
                        $('#messages').contents().filter(':gt(100)').remove();
                    }
                    for (var i = 1; i < myChart.series.length; i++)
                    {
                        if (myChart.series[i].name == eventObject.dspl)
                        {
                            break;
                        }
                    }
                    if (i >= myChart.series.length)
                    {
                        myChart.addSeries(
                            {
                                name: eventObject.dspl,
                                data: []
                            });
                    }

                    AddPointToChartSeries(myChart, i, eventObject.time, eventObject.temp);
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
        if (myChart.series[seriesIndex].points[chart.series[seriesIndex].points.length - 1].x >= lastTimeInTicks)
        {
            // The new datapoint is older than the last one we already have for this series: ignore!
            insert = false;
        }
        else
        {
            if (myChart.series[seriesIndex].points[0].x < lastTimeToShowInTicks - 5 * 60000)
            {
                shift = true;
            }
        }
    }
    if (insert)
    {
        myChart.xAxis[0].setExtremes(lastTimeToShowInTicks, null, updateChart); // Only show last n minutes
        myChart.series[seriesIndex].addPoint([lastTimeInTicks, y],
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
    for (var i = 1; i < myChart.series.length; i++)
    {
        if (newSensor == "All" || myChart.series[i].name == newSensor)
        {
            myChart.series[i].show();
        }
        else
        {
            myChart.series[i].hide();
        }
    }
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
