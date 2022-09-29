"use strict";

var songEditorTitle = $("#songEditorTitle");

var titleInput = $("#titleInput")
var artistInput = $("#artistInput")
var albumInput = $("#albumInput")
var genreInput = $("#genreInput")
var yearInput = $("#yearInput")
var indexInput = $("#indexInput")

var songLinkElem = $("#songLink");
var songDurationElem = $("#songDuration");
var songCreatedElem = $("#songCreatedDateTime");
var songAddedElem = $("#songAddedDateTime");
var songBitRateElem = $("#songBitRate");

var editorSong;

function showSongEditor(fileName) {

	editorSong = songArray.find(e => e.fileName == fileName);

	var title = editorSong.title == null ? editorSong.fileName : editorSong.title;

	songEditorTitle.text(`${title} - Song editor`);

	loadEditorInputs();
	loadEditorFields();

	ShowscreenDarker();
	SongEditor.fadeIn("fast");
}

function loadEditorInputs() {
	titleInput.val(editorSong.title);
	artistInput.val(editorSong.artist);
	albumInput.val(editorSong.album);
	genreInput.val(editorSong.genre);
	yearInput.val(editorSong.year);
	indexInput.val(editorSong.userIndex);
}

function loadEditorFields() {
	var added = getDateTimeString(editorSong.addedUtcDateTime);
	var created = getDateTimeString(editorSong.createdUtcDateTime);

	songLinkElem.text(GetContextSongFullPath())
	songDurationElem.text(editorSong.durationString)
	songCreatedElem.text(created)
	songAddedElem.text(added)
	songBitRateElem.text(editorSong.bitRate)
}

function saveSongChanges() {
	var data = createEditedSongFormData();

	fetch(`/api/music/${editorSong.fileName}`, { method: "PUT", body: data })
		.then(response => {
			if (response.ok) {
				notifySucc("Successfully updated song details!");

				var song = songArray.find(e => e.fileName == editorSong.fileName);;
				song.title = titleValue;
				song.artist = artistValue;
				song.album = albumValue;
				song.genre = genreValue;
				song.year = yearValue;
				song.userIndex = Number(userIndexValue);

				rerenderSongs();
				HideAllThePopUps();
			}
			else {
				throw "Server returned unsucessful status code!"
			}
		})
		.catch(error => {
			notifyBad(`Failed while updating song details. Error: ${error}`);
		});
}

function createEditedSongFormData() {
	var data = new FormData();

	var yearValue = yearInput.val();
	var userIndexValue = indexInput.val();
	var titleValue = titleInput.val();
	var artistValue = artistInput.val();
	var albumValue = albumInput.val();
	var genreValue = genreInput.val();

	data.append("title", titleValue)
	data.append("artist", artistValue)
	data.append("album", albumValue)
	data.append("genre", genreValue)
	data.append("year", yearValue)
	data.append("userIndex", userIndexValue);

	return data;
}

function songEditorNumInputValidation(evt) {

	var valid = onlyNumbersInputValidation(evt);

	if (!valid) {
		notifyBad("Only numbers allowed in this field!")
	}

	return valid;
}