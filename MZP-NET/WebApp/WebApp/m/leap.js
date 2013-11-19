var ws;
var paused = false;
var pauseOnGesture = false;
var presenceSince = new Date();
var secondsSince;
var isPresenceOn = true;
var s = document.createElement('div');

// Support both the WebSocket and MozWebSocket objects
if ((typeof (WebSocket) == 'undefined') &&
    (typeof (MozWebSocket) != 'undefined')) {
	WebSocket = MozWebSocket;
}

// Create the socket with event handlers
function init() {
	// Create and open the socket
	ws = new WebSocket("ws://192.168.0.10:6437/"); 

	// On successful connection
	ws.onopen = function (event) {
		var enableMessage = JSON.stringify({ enableGestures: true });
		ws.send(enableMessage); // Enable gestures
		//document.getElementById("main").style.visibility = "visible";
		document.getElementById("connection").innerHTML = "OK";
	};

	followCursor.init();
	document.body.onmousemove = followCursor.run;

	// On message received
	ws.onmessage = function (event) {
		secondsSince = Math.round((new Date() - presenceSince) / 1000);
		document.getElementById("counter").innerHTML = secondsSince;
		if (!paused) {
			var obj = JSON.parse(event.data);
			var str = JSON.stringify(obj, undefined, 2);
			//document.getElementById("output").innerHTML = '<pre>' + str + '</pre>';
			if (obj != null) {
				if (pauseOnGesture && obj.gestures.length > 0) {
					togglePause();
					//document.getElementById("gest").innerHTML += obj.gestures[0].type +';';
				}

				if (obj.gestures != null && obj.gestures.length > 0 && obj.gestures[0].duration > 500999) {
					if (obj.gestures[0].state == "stop") {
						//document.getElementById("gest").innerHTML += '.';
						if (obj.gestures[0].type == "circle") {
							//document.getElementById("leap").innerHTML = "circle";
							//next();
							//document.getElementById("leap").innerHTML = "circle:" + obj.gestures[0].duration;
						}
					}
					document.getElementById("leap1").innerHTML = "Cmd:" + obj.gestures[0].type + "-" + obj.gestures[0].state + "-" + obj.gestures[0].duration;
				}

				if (obj.gestures != null && obj.gestures.length > 0) {
					//document.getElementById("leap2").innerHTML = "Real:" + obj.gestures[0].type + "-" + obj.gestures[0].state + "-" + obj.gestures[0].duration;
				}

				if (obj.pointables != null && obj.hands != null) {
					document.getElementById("leap3").innerHTML = "Hands: " + obj.hands.length + " fingers: " + obj.pointables.length;
					if (obj.hands.length == 1) {
						var x = obj.hands[0].palmPosition[0];
						var y = obj.hands[0].palmPosition[2];
						//document.getElementById('msg').innerHTML = x + ', ' + y;
						s.style.left = 200 + (x*1.2) + 'px';
						s.style.top = 200 + (y*1.2) + 'px';
					}
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
						if (secondsSince > 30) {
							presenceOff();
							isPresenceOn = false;
						}
					}
				}
				if (obj.pointables != null) {
					//document.getElementById("point").innerHTML = obj.pointables.length + ';';
				}
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

function getMouseCoords(e) {
	var e = e || window.event;
	//document.getElementById('msg').innerHTML = e.clientX + ', ' +
    //       e.clientY + '<br>' + e.screenX + ', ' + e.screenY;
}


var followCursor = (function () {
	//var s = document.createElement('div');
	s.style.position = 'absolute';
	s.style.margin = '0';
	s.style.padding = '5px';
	s.style.border = '2px solid red';
	
	return {
		init: function () {
			document.body.appendChild(s);
		},

		run: function (e) {
			var e = e || window.event;
			//s.style.left = (e.clientX - 5) + 'px';
			//s.style.top = (e.clientY - 5) + 'px';
			getMouseCoords(e);
		}
	};
} ());
