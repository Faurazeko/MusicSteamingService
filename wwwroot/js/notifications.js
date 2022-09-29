var notifications = $("#notificationColumn");

function createNotifElem(className, text) {
	return $('<div/>', {
		html: text,
		class: `p-2 ${className} notification m-2 rounded`
	});
}

function pushNotification(notifElem, doNotHide) {

	var text = notifElem.text();
	var time = calculateTextScreenTime(text);

	notifElem
		.hide()
		.appendTo(notifications)
		.slideDown();

	if (doNotHide == false || doNotHide === undefined) {
		notifElem.delay(time).fadeOut("slow", "linear", function () { $(this).remove(); });
	}
}

function calculateTextScreenTime(text) {
	const CpmReading = 10;
	var length = text.length;

	return ((length / CpmReading) * 1000) + 1000;

}

function notifySucc(text) {
	pushNotification(createNotifElem("bg-success", text));
	console.log(`Notification success: ${text}`)
}

function notifyBad(text) {
	pushNotification(createNotifElem("bg-danger", text));
	console.log(`Notification bad: ${text}`)
}

function notifyInfo(text, elemId, doNotHide) {

	var elem = createNotifElem("bg-primary", text);
	elem.attr("id", elemId);

	pushNotification(elem, doNotHide);
	console.log(`Notification info: ${text}`)
}