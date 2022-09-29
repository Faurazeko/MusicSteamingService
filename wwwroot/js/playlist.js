//Info elements
var songsLoadingElement = $("#songsLoadingElement");
var songsEmptyElement = $("#songsEmptyElement");

//Controls info zone
var songTitle = $("#songTitle");
var songArtist = $("#songArtist");
var durationElem = $("#duration");
var curTimeElem = $("#curTime");

//Controls
var playStopBtn = $("#playBtn");
var volumeSlider = $("#volumeSlider");
var songTimeSlider = $("#songTimeSlider");
var shuffleBtn = $("#shuffleBtn");
var repeatBtn = $("#repeatBtn");
var volumeBtn = $("#volumeBtn");
var mutedVolumeBtn = $("#mutedVolumeBtn");

//Music headers
var indexHeader = $("#indexHeader");
var titleHeader = $("#titleHeader");
var artistHeader = $("#artistHeader");
var albumHeader = $("#albumHeader");
var dateAddedHeader = $("#dateAddedHeader");
var durationHeader = $("#durationHeader");

//Generic
var audio = $("audio");
var songContextMenu = $("#songContextMenu");
var contextMenu = $("#generalContextMenu");
var songList = $("#songList");
var songEditor = $("#songEditor");

//Left settings
var bitRateSlider = $("#bitRateSlider");
var bitRateSpan = $("#bitRateStringElem");
var isPlaylistPublicCheckbox = $("#isPlaylistPublicCheckbox");

var songsFilesInput = $("#songsUploadInput");
var dropFilesElem = $("#dropFilesElem");
var isDragZoneVisible = false;
var dragTimeout = -1;

var screenDarker = $("#screenDarker");

var songArray = [];

var lastSortName;

var shuffleEnabled = false;
var repeatEnabled = false;

var lastVolume = volumeSlider.val();

var currentSongIndex = -1;
var currentSongFilename = "";

var bitRate;

var popUpsList = $("#popUpsList");

var arrowsSecSkip = 5;

var isPlaylistOwner = false;

const bitRatesArray =
	[
		1000, 8000, 11025, 16000, 22050, 32000, 44100, 47250, 48000, 50000, 50400, 64000, 88200,
		96000, 176400, 192000, 352800, 2822400, 5644800, 11289600, 22579200, -1
	]

function playSongByFileName(songFileName) {

	var songIndex = songArray.findIndex(e => e.fileName == songFileName);
	playSongByIndex(songIndex);
}

function playSongByIndex(songIndex) {
	var songObj = getSongObjWithFilledFields(songArray[songIndex]);

	$(".active-song").each((i, e) => $(e).toggleClass("active-song"));

	currentSongIndex = songIndex;

	var songElement = $(songList.children()[currentSongIndex]);
	var serverPath = `/api/music/${songObj.fileName}`;
	currentSongFilename = serverPath.split("/").slice(-1)[0];

	if (bitRate > 0) {
		serverPath += `?bitrate=${bitRate}`;
	}

	songElement.addClass("active-song");


	songTitle.text(songObj.title);
	songArtist.text(songObj.artist);

	if (audio.attr("src") == serverPath) {
		toggleSong();
		return;
	}

	audio.attr("src", serverPath);

	loadMetadata(songObj.title, songObj.artist, songObj.album);
	resumeSong();
}

function loadMetadata(songTitle, songArtist, songAlbum) {
	navigator.mediaSession.metadata = new MediaMetadata({
		title: `${songTitle}`,
		artist: `${songArtist}`,
		album: `${songAlbum}`,
		artwork: [
			{ src: 'https://dummyimage.com/96x96', sizes: '96x96', type: 'image/png' },
			{ src: 'https://dummyimage.com/128x128', sizes: '128x128', type: 'image/png' },
			{ src: 'https://dummyimage.com/192x192', sizes: '192x192', type: 'image/png' },
			{ src: 'https://dummyimage.com/256x256', sizes: '256x256', type: 'image/png' },
			{ src: 'https://dummyimage.com/384x384', sizes: '384x384', type: 'image/png' },
			{ src: 'https://dummyimage.com/512x512', sizes: '512x512', type: 'image/png' },
		]
	});
}

function stopSong() {
	audio.trigger("pause");

	playStopBtn.removeClass("bi-pause-circle-fill");
	playStopBtn.addClass("bi-play-circle-fill");
}

function resumeSong() {
	if (currentSongFilename == "") {
		playSongByIndex(0);
		return;
	}

	audio.trigger("play");

	playStopBtn.removeClass("bi-play-circle-fill");
	playStopBtn.addClass("bi-pause-circle-fill");
}

function toggleSong() {
	if (audio[0].paused) {
		resumeSong();
	}
	else {
		stopSong();
	}
}

function updateAudioVolume() {
	var volume = Number(volumeSlider.val());
	localStorage.setItem("volume", volume);

	audio.prop("volume", volume);

	if (volume == 0) {
		showMutedVolumeBtn();
	}
	else {
		showVolumeBtn();
	}
}

function addToAudioVolume(number) {
	volumeSlider.val(Number(volumeSlider.val()) + number)
	updateAudioVolume();
}

function updateBitRate() {
	var selectedIndex = bitRateSlider.val();

	localStorage.setItem("bitRate", selectedIndex);

	bitRate = bitRatesArray[selectedIndex];

	var text = `${bitRate} Hz`;

	if (selectedIndex <= 0) {
		text = "Punish me, daddy!";
	}
	else if (bitRate <= 0) {
		text = "Without limits";
	}

	bitRateSpan.text(text);
}

function onAudioTimeUpdate() {

	var duration = audio.prop("duration");
	var currTime = audio.prop("currentTime");

	if (isNaN(duration)) {
		return;
	}

	songTimeSlider.val(currTime);
	songTimeSlider.prop("max", duration);

	durationElem.text(getTimeString(duration));
	curTimeElem.text(getTimeString(currTime));

	if (duration == currTime) {
		nextSong();
	}
}

function nextSong() {
	var index;

	if (repeatEnabled) {
		audio.prop("currentTime", 0);
		return;
	}
	else if (shuffleEnabled) {
		index = randomNumberFromInterval(0, songArray.length - 1);
	}
	else
	{
		index = currentSongIndex + 1;

		if (index >= songArray.length) {
			index = 0
		}
	}

	playSongByIndex(index);
}

function prevSong() {

	if (repeatEnabled) {
		audio.prop("currentTime", 0);
		return;
	}

	if (audio.prop("currentTime") > 5) {
		audio.prop("currentTime", 0);
		return;
	}

	var index = currentSongIndex - 1;

	if (index < 0) {
		index = songArray.length - 1;
	}

	playSongByIndex(index);
}

function onSongTimeInput() {
	audio.prop("currentTime", songTimeSlider.val());
	resumeSong();
}

function shuffleBtnClick() {
	shuffleBtn.toggleClass("kinda-btn-toggle");
	shuffleBtn.toggleClass("kinda-btn");

	shuffleEnabled = !shuffleEnabled;
}

function repeatBtnClick() {
	repeatBtn.toggleClass("kinda-btn-toggle");
	repeatBtn.toggleClass("kinda-btn");

	repeatEnabled = !repeatEnabled;
}

function volumeBtnClick() {
	var storedVolumeValue = volumeSlider.val();
	volumeSlider.val(0)
	updateAudioVolume();
	lastVolume = storedVolumeValue;
}

function mutedVolumeBtnClick() {
	volumeSlider.val(lastVolume)
	updateAudioVolume();
}

function showVolumeBtn() {
	volumeBtn.css("visibility", "inherit")
	mutedVolumeBtn.css("visibility", "collapse")
}

function showMutedVolumeBtn() {
	volumeBtn.css("visibility", "collapse")
	mutedVolumeBtn.css("visibility", "inherit")
}

function downloadCurrentSong() {
	if (currentSongFilename == "") {
		return;
	}

	var link = `/api/music/download/${currentSongFilename}`;
	window.open(link, '_blank').focus();
}

function downloadContextMenuSong() {
	var link = `/api/music/download/${songContextMenu.attr("data-filename")}`;
	window.open(link, '_blank').focus();
}

function deleteSongByFilename(filename) {

	var arrayElem = songArray.find((element) => element.fileName == filename);
	var arrayIndex = songArray.indexOf(arrayElem);

	fetch(`/api/music/${filename}`,
		{
			method: "DELETE"
		})
		.then(response => {
			if (response.ok) {
				notifySucc("Song successfully deleted!");

				if (currentSongIndex == arrayIndex) {
					nextSong();
				}

				removeSong(arrayIndex);

			}
			else {
				throw "server returned bad status code :(";
			}
		})
		.catch(error => {
			notifyBad(`Error while deleting song :( Error: ${error}`);
		})
}

function removeSong(arrayIndex) {
	songArray.splice(arrayIndex, 1);
	unrenderSongByIndex(arrayIndex);
}

function deleteCurrentSong() {
	deleteSongByFilename(currentSongFilename);
}

function deleteSongContextMenu() {
	deleteSongByFilename(songContextMenu.attr("data-filename"), songContextMenu.attr("arrayIndex"))
}

function uploadFiles() {
	var data = new FormData($("#uploadFilesForm")[0]);

	var size = calculateFormDataSize(data);

	if (size > 100000000) {
		notifyBad(`Files is too big (${size} bytes)! Total size of all files must be less than 100 MB!`);
		return;
	}

	var notificationElemId = `UploadNotification${generateRandomHex()}`;
	var progressElemId = `UploadProgress${generateRandomHex()}`;

	var text = "Uploading your files!";

	text +=
		`<div class="progress" style="width:100%;"> ` +
			`<div id="${progressElemId}" class="progress-bar bg-success" style="width:0%;">0%</div >` +
		`</div>`;

	notifyInfo(text, notificationElemId, true)

	var progress = $("#" + progressElemId);
	var notification = $("#" + notificationElemId);

	var hideNotif = function () {
		notification.fadeOut("slow", "linear", function () { $(this).remove(); });
	}

	var uplaodFailed = function () {
		notifyBad(`Failed while uploading songs. Server returned error status code.`);

		hideNotif();
	}

	var request = new XMLHttpRequest();

	request.open("POST", "/api/music");

	request.upload.addEventListener("progress", function (e) {
		var percentComplete = Math.trunc((e.loaded / e.total) * 100);
		var percentText = `${percentComplete}%`

		progress.css({ "width": percentText });
		progress.text(percentText);
	});

	request.addEventListener("load", function () {

		if (request.status != 201) {
			uplaodFailed();
			return;
		}

		hideNotif();
		notifySucc("Files uploaded! Music is in your library now!");

		var response = JSON.parse(request.responseText);

		response.forEach(song => addSong(song));
	})

	request.addEventListener("error", function () {
		uplaodFailed();
	})

	request.send(data)
}

function copyContextSongLink() {
	copyToClipboard(getContextSongFullPath());
}

function getContextSongFullPath() {
	return hostName + songContextMenu.attr("data-path")
}

function loadSongs() {

	const params = new URLSearchParams(window.location.search);

	var link = `/api/music`;

	if (params.has("playlist")) {
		link += `?playlist=${params.get("playlist")}`;
	}

	fetch(link, { method: "GET" })
		.then(async response => {
			if (!response.ok) {
				throw "server returned error code.";
			}

			return await response.json()
		})
		.then(json => {
			setSongs(json.songs);
			$("#playlistTitle").text(`${json.ownerUsername}'s playlist`);
			$("#playlistSongsCount").text(`There is ${json.songs.length} song(s) in playlist.`);
			$("#playlistDuration").text(`Duration: ${json.durationString}`);
			isPlaylistPublicCheckbox.prop("checked", json.isPlaylistPublic);
		})
		.catch(error => {
			notifyBad(`Failed while loading music list: ${error}`);
			$("#songsErrorElement").fadeIn("slow");
		});

	songsLoadingElement.slideUp();
}

function setSongs(array) {
	array.forEach(song => {
		addSong(song)
	});

	sortSongs("UserIndex")

	if (songArray.length <= 0) {
		songsEmptyElement.slideDown()
	}
}

function addSong(songObj) {
	songObj.title = songObj.title == "" ? null : songObj.title;
	songObj.artist = songObj.artist == "" ? null : songObj.artist;
	songObj.album = songObj.album == "" ? null : songObj.album;

	songArray.push(songObj);

	renderSong(songObj);

	if (songArray.length > 0) {
		songsEmptyElement.slideUp()
	}
}

function showSongContextMenu(event, elem) {
	contextMenu.hide();

	songContextMenu.attr("data-filename", elem.getAttribute("data-filename"))
	songContextMenu.attr("data-path", elem.getAttribute("data-path"))
	songContextMenu.attr("data-elementId", "#" + elem.getAttribute("id"))

	showContextMenu(songContextMenu, event);
}

function showGenericContextMenu() {
	songContextMenu.hide();

	showContextMenu(contextMenu, event);
}

function showContextMenu(element, event) {

	var myWindow = $(window);

	var windowHeight = myWindow.height();
	var windowWidth = myWindow.width();

	var maxY = windowHeight - (event.pageY + element.height());
	var maxX = windowWidth - (event.pageX + element.width());

	var corY = event.pageY;
	var corX = event.pageX;

	if (maxY < 0) {
		corY = event.pageY - element.height() - 15;
	}

	if (maxX < 0) {
		corX = event.pageX - element.width();
	}

	element.css("top", corY + 'px');
	element.css("left", corX + 'px');

	element.show();
}

function sortSongs(sortName) {

	var sortFunction;
	var header;

	switch (sortName) {
		case "UserIndex":
			header = indexHeader;

			sortFunction = (a, b) => {
				if (a.userIndex == b.userIndex) {
					return 0;
				}

				return a.userIndex > b.userIndex;
			}
			break;
		case "Title":
			header = titleHeader;

			sortFunction = (a, b) => {
				if (a.title < b.title) {
					return -1;
				}
				else if (a.title > b.title) {
					return 1;
				}
				else {
					return 0;
				}
			}
			break;
		case "Artist":
			header = artistHeader;

			sortFunction = (a, b) => {
				if (a.artist < b.artist) {
					return -1;
				}
				else if (a.artist > b.artist) {
					return 1;
				}
				else {
					return 0;
				}
			}
			break;
		case "Album":
			header = albumHeader;

			sortFunction = (a, b) => {
				if (a.album < b.album) {
					return -1;
				}
				else if (a.album > b.album) {
					return 1;
				}
				else {
					return 0;
				}
			}
			break;
		case "Duration":
			header = durationHeader;

			sortFunction = (a, b) => {
				if (a.duration < b.duration) {
					return -1;
				}
				else if (a.duration > b.duration) {
					return 1;
				}
				else {
					return 0;
				}
			}
			break;
		case "DateAdded":
		default:
			header = dateAddedHeader;

			sortFunction = (a, b) => {
				if (a.addedUtcDateTime < b.addedUtcDateTime) {
					return -1;
				}
				else if (a.addedUtcDateTime > b.addedUtcDateTime) {
					return 1;
				}
				else {
					return 0;
				}
			}
			break;
	}

	var sortIndexer = $("#sortIndexer")


	if (lastSortName == sortName) {
		songArray.reverse();
		sortIndexer.toggleClass("flip")
	}
	else {
		songArray.sort(sortFunction);

		sortIndexer.remove();

		var indexer = document.createElement("i");
		indexer.id = "sortIndexer"
		indexer.className = "bi bi-caret-up-fill";

		var innerElem = header.children()[0]

		var scrollWidth = innerElem.scrollWidth + innerElem.offsetLeft;

		indexer.style.cssText = `left: ${scrollWidth}px; position: absolute;`

		header.append(indexer);
	}

	lastSortName = sortName;

	rerenderSongs();
}

function updateHostElem() {
	var hostElem = $("#hostElem");
	hostElem.text(location.host);
	hostElem.attr("href", hostName);
}

function showchangeVisibilityWarning() {
	showScreenDarker();
	$("#changeVisibilityWarning").fadeIn("fast");
}

function acceptChangingVisibility() {
	hideAllThePopUps();
	isPlaylistPublicCheckbox.prop("checked", true);
	changePlaylistVisibility();
	copyPlaylistLink();
}

function showScreenDarker() {
	screenDarker.fadeIn("fast");
}

function hideAllThePopUps() {

	popUpsList.children().each((i, e) => $(e).fadeOut("fast"));
}

function showDragNDropElem() {
	isDragZoneVisible = true;
	dropFilesElem.fadeIn("fast");
}

function hideDragNDropElem() {
	isDragZoneVisible = false;
	clearTimeout(dragTimeout);

	dragTimeout = setTimeout(function () {
		if (!isDragZoneVisible) { dropFilesElem.fadeOut("fast"); }
	}, 200);
}

function preventDefaults(e) {
	e.stopPropagation()
	e.preventDefault()
}

function handleFilesDrop(e) {
	songsFilesInput.prop("files", e.dataTransfer.files)
	uploadFiles();
}

function addDragNDropFunctinaity() {
	["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
		document.addEventListener(eventName, preventDefaults, false)
	});

	["dragenter", "dragover"].forEach(eventName => {
		$("html").on(eventName, showDragNDropElem);
	});

	["dragleave", "drop"].forEach(eventName => {
		$("html").on(eventName, hideDragNDropElem);
	});

	document.addEventListener("drop", handleFilesDrop, false);
}

function changePlaylistVisibility() {

	fetch(`/api/user/playlist?visibility=${isPlaylistPublicCheckbox.prop("checked")}`, { method: "PUT" })
		.then(response => {
			if (!response.ok) {
				throw "Server returned error status code :("
			}
			notifySucc("Successfully changed the playlist visibility");
		})
		.catch(error => {
			isPlaylistPublicCheckbox.prop("checked", !isPlaylistPublicCheckbox.prop("checked"));
			notifyBad(`Failed to change visibility of the playlist. Error: ${error}`);
		})
}

function copyPlaylistLink() {

	var playlistName = location.href.split("=").at(-1);
	var username = $("#username").text();

	if (location.href == playlistName) {
		playlistName = username;
	}

	var link = `${hostName}?playlist=${playlistName}`;

	if (isPlaylistOwner) {
		if (isPlaylistPublicCheckbox.prop("checked") == false) {
			showchangeVisibilityWarning();
		}
		else {
			copyToClipboard(link);
		}
	}
	else {
		copyToClipboard(link);
    }
}

function addHotKeys() {
	$(document).on("keydown", (event) => {

		if (event.target.tagName.toLowerCase() === "input") {
			return;
		}

		switch (event.code) {
			case "KeyD":
				downloadCurrentSong();
				break;
			case "KeyJ":
				prevSong();
				break;
			case "KeyK":
				toggleSong();
				break;
			case "KeyL":
				nextSong();
				break;
			case "Space":
				stopSong();
				break;
			case "KeyM":

				if (mutedVolumeBtn.css("visibility") == "collapse") {
					volumeBtnClick();
				}
				else {
					mutedVolumeBtnClick();
				}
				break;
			case "ArrowRight":
				audio.prop("currentTime", audio.prop("currentTime") + arrowsSecSkip)
				break;
			case "ArrowLeft":
				audio.prop("currentTime", audio.prop("currentTime") - arrowsSecSkip)
				break;
			case "ArrowUp":
				addToAudioVolume(0.01);
				break;
			case "ArrowDown":
				addToAudioVolume(-0.01);
				break;
			default:
				//alert(event.code)
				break;
		}
	});
}

function editElementsForNonOwner() {

	if (isPlaylistOwner) {
		return;
	}

	$(".onlyForOwner").each((i, e) => {

		var je = $(e)

		var tagName = je.prop("tagName").toLowerCase();

		if (tagName != "label" && tagName != "button") {
			je.css("color", "gray");
		}

		if (je.hasClass("btn")) {
			je.prop("class", "btn btn-secondary")
		}

		je.css("pointer-events", "none");
		je.prop("disabled", true)
		je.prop("for", "")
	});

	$(".onlyForNonOwner").each((i, e) => {
		$(e).show();
    })

}

function addSongIntoPlaylist(filename) {

	fetch(`/api/music/existing?filename=${filename}`, { method: "POST" })
		.then(r => {
			if (!r.ok) {
				throw "Server returned error status code";
			}

			notifySucc("Successfully added song to your playlist!");
		})
		.catch(e => notifyBad(`Failed to add this song to your playlist. Error: ${e}`));
}

$(document).ready(function () {

	var playlistName = location.href.split("=").at(-1);
	var username = $("#username").text();

	if (location.href == playlistName) {
		playlistName = username;
		isPlaylistOwner = true;
    }

	$("#playlistNameInput").val(playlistName)


	if (playlistName == username) {
		isPlaylistOwner = true;
	}

	editElementsForNonOwner();


	$("html").on("mousedown", (event) => {

		if ($(event.target).closest(".song-item").length > 0) {
			return;
		}

		if (event.button != 2) {
			return;
		}

		showGenericContextMenu();
	});

	addDragNDropFunctinaity();

	var volume = localStorage.getItem("volume");
	if (volume == null) {
		localStorage.setItem("volume", 1);
		volume = 1;
	}

	volumeSlider.val(volume);
	updateAudioVolume();

	var bitRate = localStorage.getItem("bitRate");
	if (bitRate == null) {
		localStorage.setItem("bitRate", 100);
		bitRate = 100;
	}

	addHotKeys();
	bitRateSlider.val(bitRate)
	updateBitRate();

	updateHostElem();
	loadSongs();


	navigator.mediaSession.setActionHandler('play', resumeSong);
	navigator.mediaSession.setActionHandler('pause', stopSong);
	navigator.mediaSession.setActionHandler('previoustrack', prevSong);
	navigator.mediaSession.setActionHandler('nexttrack', nextSong);
	navigator.mediaSession.setActionHandler('stop', stopSong);
})

$(document).bind("contextmenu", function (e) { return false; })

// hide context
$(document).bind("click", function (e)
{
	songContextMenu.hide();
	contextMenu.hide();
})
