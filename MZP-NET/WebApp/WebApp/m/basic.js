function refresh(interval) {
	setTimeout(function () { reload() }, interval);
}
function reload() {
	window.location.reload();
}
function ajaxCall(targetDiv,url) {
	var xmlhttp;
	if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp = new XMLHttpRequest();
	}
	else {// code for IE6, IE5
		xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
	}
	document.getElementById(targetDiv).innerHTML = "Executing Call:" + url;
	xmlhttp.onreadystatechange = function () {
		if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
			document.getElementById(targetDiv).innerHTML = xmlhttp.responseText;
		}
	}
	xmlhttp.open("GET", url, true);
	xmlhttp.send();
}