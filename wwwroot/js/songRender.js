"use strict"


function renderSong(song, isActive) {

	var songObj = getSongObjWithFilledFields(song)

	var arrayElem = songArray.find((element) => element.fileName == songObj.fileName);
	var arrayIndex = songArray.indexOf(arrayElem);

	var songIndex = songObj.userIndex;

	var songElem = createSongElem(isActive, songObj.path, arrayIndex);

	var elems =
		[
			createIndexElem(songIndex), createTitleArtistElem(songObj.title, songObj.artist), createAlbumElem(songObj.album),
			createDateAddedElem(songObj.addedUtcDateTime), getDurationElem(songObj.durationString)
		];

	elems.forEach(e => songElem.append(e));

	$(songElem).on("mousedown", (event) => {
		if (event.button != 2) {
			return;
		}

		var elem = event.currentTarget;
		showSongContextMenu(event, elem);
	});

	songList.append(songElem);
}

function createSongElem(isActive, songPath, arrayIndex) {

	var songElem = document.createElement("div");

	var classList = "row py-1 menu-item-frame text-wrap text-break song-item ";

	if (isActive) {
		classList += "active-song"
	}
	songElem.className = classList


	songElem.setAttribute("data-path", songPath)
	songElem.setAttribute("data-filename", songPath.split("/").slice(-1)[0])

	songElem.onclick = () => playSongByIndex(arrayIndex);

	return songElem;
}

function createIndexElem(index) {
	var indexElem = document.createElement("div");
	indexElem.className = "col-sm-1 text-center song-index";
	indexElem.style = "display: flex; justify-content: center; align-items: center; height: 3rem; font-size: 1.2vw;";
	indexElem.textContent = index;

	return indexElem;
}

function createTitleArtistElem(title, artist) {
	var TitleArtistElem = document.createElement("div");
	TitleArtistElem.className = "col-sm-4";

	TitleArtistElem.append(createTitleElem(title));

	TitleArtistElem.append(createArtistElem(artist));

	return TitleArtistElem;
}

function createTitleElem(title) {
	var titleElem = document.createElement("p");
	titleElem.className = "my-0";
	titleElem.innerText = title;

	return titleElem;
}

function createArtistElem(artist) {
	var artistElem = document.createElement("p");
	artistElem.style = "color:gray;";
	artistElem.className = "my-0";
	artistElem.innerText = artist;

	return artistElem;
}

function createAlbumElem(album) {
	var albumElem = document.createElement("div");
	albumElem.className = "col-sm-3";
	albumElem.style = "display: flex; align-items: center; min-height: 3rem;";
	albumElem.innerText = album;

	return albumElem;
}

function createDateAddedElem(addedUtcDateTime) {
	var dateAddedElem = document.createElement("div");
	dateAddedElem.className = "col-sm-2 text-center";
	dateAddedElem.style = "display: flex; justify-content: center; align-items: center; height: 3rem;";
	dateAddedElem.innerText = getAddedDateTimeString(addedUtcDateTime);

	return dateAddedElem;
}

function getAddedDateTimeString(dateTime) {
	return new Date(dateTime).toLocaleString("en-US", { year: "numeric", month: "short", day: "2-digit" });
}

function getDurationElem(durationString) {
	var durationElem = document.createElement("div");
	durationElem.className = "col-sm-2 text-center";
	durationElem.style = "display: flex; justify-content: center; align-items: center; height: 3rem;";
	durationElem.innerText = durationString;

	return durationElem;
}

function reRenderSongsIndexes() {
	$(".song-index").each(function (index) {
		$(this).text(index + 1);
	});
}

function rerenderSongs() {

	songList.empty();

	songArray.forEach((song, index) => {

		var isActive = false;

		if (song.fileName == currentSongFilename) {
			currentSongIndex = index;
			isActive = true;
		}

		renderSong(song, isActive);
	});

	reRenderSongsIndexes();
}

function unrenderSongByIndex(arrayIndex) {

	songList.children()[arrayIndex].remove();

	if (songArray.length <= 0) {
		songsEmptyElement.slideDown();
		return;
	}

	reRenderSongsIndexes();
}