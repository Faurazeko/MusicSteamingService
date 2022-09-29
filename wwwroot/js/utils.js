"use stict";

var hostName = location.protocol + '//' + location.host;

function getSongObjWithFilledFields(songObj) {
	var obj = structuredClone(songObj);

	obj.title = songObj.title == null ? songObj.fileName.split(".")[0] : songObj.title;
	obj.artist = songObj.artist == null ? "???" : songObj.artist;
	obj.album = songObj.album == null ? "???" : songObj.album;

	return obj;
}

function generateRandomHex() {
	return Math.floor(Math.random() * 16777215).toString(16);
}

function generateRandomHtmlColor() {
	return `#${generateRandomHex()}`;
}

function copyToClipboard(object) {
	navigator.clipboard.writeText(object);
	notifySucc("Successfully copied to clipboard!")
}

function getTimeString(timeNumber) {

	var result;

	var hours = Math.floor(timeNumber / 3600);
	var minutes = Math.floor((timeNumber / 60) - (hours * 60));
	var seconds = Math.floor(timeNumber % 60);

	if (seconds < 10) {
		seconds = `0${seconds}`
	}

	if (hours > 0) {

		if (minutes < 10) {
			minutes = `0${minutes}`;
		}

		result = `${hours}:${minutes}:${seconds}`;
	}
	else {
		result = `${minutes}:${seconds}`;
	}

	return result;
}

function getDateTimeString(dateTimeString) {
	var date = new Date(dateTimeString).toLocaleString("en-US", { year: "numeric", month: "short", day: "2-digit" });
	var time = new Date(dateTimeString).toLocaleTimeString("ru-RU");

	var result = `${date} ${time}`;
	return result;
}

function onlyNumbersInputValidation(evt) {
	var ASCIICode = (evt.which) ? evt.which : evt.keyCode
	if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
		return false;
	return true;
}

function randomNumberFromInterval(min, max) {
	return Math.floor(Math.random() * (max - min + 1) + min)
}

function calculateFormDataSize(formData) {
	var size = 0;

	for (const pair of formData.entries()) {

		var prop = pair[1];

		var tempSize = typeof prop === "string" ? prop.length : prop.size;

		size += tempSize;
	}

	return size;
}