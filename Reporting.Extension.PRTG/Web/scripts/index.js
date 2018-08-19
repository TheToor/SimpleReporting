var APIUrl = "http://localhost:9000";

google.charts.load('current', { 'packages': ['corechart'] });
google.charts.setOnLoadCallback(Ready);

function Ready() {
    GetResponse().done(function (data) {
        DrawErrorChart('chart-1', data.Errors);
        DrawWarningChart('chart-2', data.Warnings);
        DrawOKChart('chart-3', data.OK);
        DrawTable(data.Sensors);
    });
}

function GetResponse() {
    return $.ajax({
        method: "GET",
        url: APIUrl + "/alarm/1",
        dataType: 'json'
    }).fail(function (xhr, status) {
        console.error("Failed to get JSON: " + status);
        $("#error").show();
    });
}

function DrawErrorChart(elementName, count) {
    DrawChart(elementName, count, 'Errors', 'red');
}

function DrawWarningChart(elementName, count) {
    DrawChart(elementName, count, 'Warnings', '#ffeb3b');
}

function DrawOKChart(elementName, count) {
    DrawChart(elementName, count, 'OK', '#4caf50');
}

function DrawChart(elementName, count, name, color) {

    var data = google.visualization.arrayToDataTable([
        [name, ''],
        [name, count]
    ]);

    var options = {
        pieHole: 0.4,
        pieSliceText: 'value',
        pieSliceTextStyle: {
            color: 'white',
            fontSize: 60
        },
        pieSliceBorderColor: color,
        slices: {
            0: { color: color }
        },
        backgroundColor: {
            fill: 'transparent'
        },
        legend: 'none',
        enableInteractivity: false
    };

    var chart = new google.visualization.PieChart(document.getElementById(elementName));

    chart.draw(data, options);
}

function DrawTable(sensors) {
    for (var i = 0; i < sensors.length; i++) {
        var item = sensors[i];
        $("table tbody").append("<tr><td>" + item.S + "</td><td>" + item.N + "</td></tr>");
    }
}