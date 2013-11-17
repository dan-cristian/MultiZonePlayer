var ws;
var paused = false;
var pauseOnGesture = false;
var presenceSince = new Date();
var secondsSince;
var isPresenceOn = true;

// Support both the WebSocket and MozWebSocket objects
if ((typeof (WebSocket) == 'undefined') &&
    (typeof (MozWebSocket) != 'undefined')) {
	WebSocket = MozWebSocket;
}

// Create the socket with event handlers
function init() {
	// Create and open the socket
	ws = new WebSocket("ws://localhost:6437/");

	// On successful connection
	ws.onopen = function (event) {
		var enableMessage = JSON.stringify({ enableGestures: true });
		ws.send(enableMessage); // Enable gestures
		//document.getElementById("main").style.visibility = "visible";
		document.getElementById("connection").innerHTML = "OK";
	};

	// On message received
	ws.onmessage = function (event) {
		if (!paused) {
			var obj = JSON.parse(event.data);
			var str = JSON.stringify(obj, undefined, 2);
			//document.getElementById("output").innerHTML = '<pre>' + str + '</pre>';
			if (pauseOnGesture && obj.gestures.length > 0) {
				togglePause();
				//document.getElementById("gest").innerHTML += obj.gestures[0].type +';';
			}

			if (obj.gestures != null && obj.gestures.length > 0) {
				if (obj.gestures[0].state == "stop" && obj.gestures[0].duration > 500999) {
					//document.getElementById("gest").innerHTML += '.';
					if (obj.gestures[0].type == "circle") {
						document.getElementById("leap").innerHTML = "circle";
						next();
						document.getElementById("leap").innerHTML = "circle:" + obj.gestures[0].duration;
					}
				}
				else
					document.getElementById("leap").innerHTML = obj.gestures[0].type + "-" + obj.gestures[0].state;
			}
				
			if (obj.hands != null && obj.hands.length == 1 && obj.pointables != null && obj.pointables.length == 5) {
				//document.getElementById("hand").innerHTML = obj.hands.length + ';';
				presenceOn();
				isPresenceOn = true;
				presenceSince = new Date();
			}
			else {
				if (isPresenceOn) {
					secondsSince = Math.round((new Date() - presenceSince) / 1000);
					if (secondsSince > 10) {
						presenceOff();
						isPresenceOn = false;
					}
				}
			}
			if (obj.pointables != null) {
				//document.getElementById("point").innerHTML = obj.pointables.length + ';';
			}
		}
	};

	// On socket close
	ws.onclose = function (event) {
		ws = null;
		//document.getElementById("main").style.visibility = "hidden";
		document.getElementById("connection").innerHTML = "Nok";
	}

	// On socket error
	ws.onerror = function (event) {
		document.getElementById("connection").innerHTML = "Received error";
	};
}

function togglePause() {
	paused = !paused;

	if (paused) {
		document.getElementById("pause").innerText = "Resume";
	} else {
		document.getElementById("pause").innerText = "Pause";
	}
}

function pauseForGestures() {
	if (document.getElementById("pauseOnGesture").checked) {
		pauseOnGesture = true;
	} else {
		pauseOnGesture = false;
	}
}