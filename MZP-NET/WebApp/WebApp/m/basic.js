function refresh(interval) {
	window.scrollTo(0, 1);
	setTimeout(function () { reload() }, interval);
}
function refresh(interval, url) {
	setTimeout(function () { reload(url) }, interval);
}
function reload() {
	window.location.reload();
}
function reload(url) {
	window.open(url, "_self");
}
function ajaxCall(targetDiv,url,forceReload) {
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
			if (forceReload) reload();
		}
	}
	xmlhttp.open("GET", url, true);
	xmlhttp.send();
}
function showText(targetDiv, text) {
	document.getElementById(targetDiv).innerHTML = text;
}
function showDiv(targetDiv) {
	document.getElementById(targetDiv).style.display = "block";
}