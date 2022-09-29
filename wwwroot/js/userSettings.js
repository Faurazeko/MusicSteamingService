"use stict";

var visibilityCheckBox = $("#visibilityCheckBox");
var newPasswordInput = $("#newPasswordInput");

newPasswordInput.val("")

function loadSettings() {
    fetch("/api/user", { method: "GET" })
        .then(respone => respone.json())
        .then(data => {
            visibilityCheckBox.prop("checked", data.isPlaylistPublic);
        })
        .catch(error => notifyBad(`Failed to load playlist visibility :(\nError: ${error}`))
}

loadSettings();


function changePlaylistVisibility() {
	fetch(`/api/user/playlist?visibility=${visibilityCheckBox.prop("checked")}`, { method: "PUT" })
        .then(response => {
            if (!response.ok) {
                throw "Server returned bad status code";
            }

            notifySucc("Changes have been saved!");
        })
        .catch(err => {
            notifyBad(`Failed to save changes ;( Error: ${err}`);

            var checked = !visibilityCheckBox.prop("checked");

            visibilityCheckBox.prop("checked", checked)
        })
}

function updatePassword() {

    var newPassword = newPasswordInput.val();
    var length = newPassword.length;

    if (length < 8 || length > 255) {
        notifyBad("Password length should be grater than 8 and no more than 255.")
        return;
    }

    fetch(`/api/user?newPassword=${newPassword}`, { method: "PUT" })
        .then(response => {
            if (!response.ok) {
                throw "Server returned bad status code";
            }

            notifySucc("Changes have been saved!");

            window.location.replace("/logout");
        })
        .catch(err => {
            notifyBad(`Failed to save changes ;( Error: ${err}`);
        })
}

function deleteAccount() {
    fetch("api/user", { method: "DELETE" })
        .then(response => {
            if (!response.ok) {
                throw "Server returned bad status code";
            }

            window.location.replace("/logout");
        })
        .catch(err => {
            notifyBad(`Failed to save changes. Maybe it's for the better? Error: ${err}`);
        })
}