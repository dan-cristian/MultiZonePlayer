
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
   	var maxW = myWidth - 5;
   	var maxH = myHeight - 5;
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
   }

   function scaleSize(maxW, maxH, currW, currH) {
   	

   }