function refresh(interval) {
	window.scrollTo(0, 1);
	setTimeout(function () { reload() }, interval);
}
function refresh(interval, url) {
	setTimeout(function () { reload(url) }, interval);
}
function reload() {
	parent.window.location.reload(true);
}
function reload(url) {
	window.open(url, "_self");
}
function ajaxConfirmCall(targetDiv, url, forceReload) {
	if (confirm("Confirm running action " + url))
		ajaxCall(targetDiv, url, forceReload);
}
function ajaxCall(targetDiv,url,forceReload) {
	var xmlhttp, div;
	if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp = new XMLHttpRequest();
	}
	else {// code for IE6, IE5
		xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
	}
	div = document.getElementById(targetDiv);
	if (div != null) div.innerHTML = "Calling:" + url;
	xmlhttp.onreadystatechange = function () {
		if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
			if (div != null) div.innerHTML = xmlhttp.responseText;
			if (forceReload) {
				reload(document.URL);
				//if (div != null) div.innerHTML = div.innerHTML + " and reloaded";
			}
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
function hideDiv(targetDiv) {
	document.getElementById(targetDiv).style.display = "none";
}