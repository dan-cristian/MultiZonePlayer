
    function refresh(interval) {
            setTimeout(function () { reload() }, interval);
        }
        function reload() {
            window.location.reload(); 
        }

       
function redirect(location){
    window.location.replace (location);
}

function redirectdelayed(location, interval) {
    setTimeout(function () { redirect(location) }, interval);
}


// return the value of the radio button that is checked
// return an empty string if none are checked, or
// there are no radio buttons
function getCheckedRadioValue(radioObj) {
    if (!radioObj || radioObj==undefined)
        return "";
    var radioLength = radioObj.length;
    if (radioLength == undefined)
        if (radioObj.checked)
            return radioObj.value;
        else
            return "";
    for (var i = 0; i < radioLength; i++) {
        if (radioObj[i].checked) {
            return radioObj[i].value;
        }
    }
    return "";
   }

   function resizeImage(obj) {
   	obj.style.visibility = "hidden";
   	var myWidth = 0, myHeight = 0;
   	if (typeof (window.innerWidth) == 'number') {
   		//Non-IE
   		myWidth = window.innerWidth;
   		myHeight = window.innerHeight;
   	} else if (document.documentElement && (document.documentElement.clientWidth || document.documentElement.clientHeight)) {
   		//IE 6+ in 'standards compliant mode'
   		myWidth = document.documentElement.clientWidth;
   		myHeight = document.documentElement.clientHeight;
   	} else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
   		//IE 4 compatible
   		myWidth = document.body.clientWidth;
   		myHeight = document.body.clientHeight;
   	}
   	var maxW = myWidth - 0;
   	var maxH = myHeight - 4;
   	var resultW, resultH;

	var ratio = Math.min(maxW / obj.width, maxH / obj.height);
	if ((ratio == maxH / obj.height)) {
		resultW = obj.width * ratio;
   		resultH= maxH;
   	} else {
   		resultW= maxW;
   		resultH= obj.height * ratio;
   	}
   	obj.width = resultW;
   	obj.height = resultH;
   	obj.style.visibility = "visible";
   }

   function reloadImage(obj, delay) {
   	setTimeout(function () { getnextframe(obj) }, delay);
   }

   function getnextframe(obj) {
   		var _url = obj.src;
   		_url = _url.substring(0, _url.indexOf('&r='));
   		_url += "&r=" + Math.random();
   		obj.src = _url;
   	}

   	function toggleDiv(showHideDiv, switchTextDiv, restoreText, collapseText) {
   		var ele = document.getElementById(showHideDiv);
   		var text = document.getElementById(switchTextDiv);
   		if (ele.style.display == "block") {
   			ele.style.display = "none";
   			text.innerHTML = restoreText;
   		}
   		else {
   			ele.style.display = "block";
   			text.innerHTML = collapseText;
   		}
   	}

   	function reloadResizeImage(obj, delay) {
   		resizeImage(obj);
   		setTimeout(function () { getnextframe(obj) }, delay);
   	}