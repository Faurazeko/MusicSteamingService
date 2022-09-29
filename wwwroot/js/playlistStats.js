"use strict";

Chart.defaults.color = "#ffffff";

var playlistStats = $("#playlistStats");

var artistChartElem = $("#artistChart");
var genreChartElem = $("#genreChart");

var artistChart;
var genreChart;


function showStats() {

	if (songArray.length <= 0) {
		notifyBad("You dont have songs in your library to analyze or songs arent loaded yet.");
		return;
	}

	showScreenDarker();
	playlistStats.fadeIn("fast", renderStats());
}

function renderStats() {
	renderArtistsChart();
	renderGenreChart();
}

function renderArtistsChart() {
	var ctx = artistChartElem[0].getContext('2d');

	var artists = getValueCountInObjByPropName(songArray, "artist");

	artistChart = getChart(ctx, artists, "Artist distribution");;
}

function renderGenreChart() {
	var ctx = genreChartElem[0].getContext('2d');

	var genres = getValueCountInObjByPropName(songArray, "genre");

	genreChart = getChart(ctx, genres, "Genre distribution");
}

function getValueCountInObjByPropName(array, objPropName) {

	var result = { "Unknown": 0 };

	array.forEach((e) => {

		var prop = e[objPropName];

		if (prop == null || prop == " " || prop == "") {
			result["Unknown"] += 1;
		}
		else if (result[prop] === undefined) {
			result[prop] = 1;
		}
		else {
			result[prop] += 1
		}
	});

	return result;
}

function getChart(context2d, dataDictionary, tableLable) {

	var lables = [];
	var numbers = [];
	var colors = ["#A9A9A9"]

	for (const [key, value] of Object.entries(dataDictionary)) {
		lables.push(key);
		numbers.push(value);
		colors.push(generateRandomHtmlColor())
	}

	var data = {
		labels: lables,

		datasets: [{
			label: tableLable,
			data: numbers,
			backgroundColor: colors,
			color: "#ff0000",
			hoverOffset: 0,
			borderColor: "#102B3F",
			borderAlign: "inner"
		}]
	};

	var config = {
		type: 'doughnut',
		data: data,
		options: getDefaultChartOptions()
	};

	return new Chart(context2d, config);
}

function getDefaultChartOptions() {
	var options = {
		plugins: {
			legend: {
				labels: {
					font: {
						size: 15
					}
				}
			}
		}
	}

	return options;
}