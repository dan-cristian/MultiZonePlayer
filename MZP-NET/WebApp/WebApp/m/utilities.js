
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