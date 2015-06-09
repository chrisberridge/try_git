//var hostBase = "http://localhost:62670";
var hostBase = "https://seguro.elcolombiano.com/realperson";

showRecaptcha= function(element) {
    Recaptcha.create("6LcTFvASAAAAAM5FNWtErk2cdjQ8ULO5NrafTXK2", element, {
        theme: "clean",
		lang : 'es',
        callback: Recaptcha.focus_response_field});
}


setup=function() {
    showRecaptcha('recaptcha_div');
}
doCaptchaValidate=function() {
    var urlToFetch = hostBase + "/rprecaptcha.aspx";
    var serverResponse = -1;
	$.support.cors = true; // Required for IE.
	$.ajax({
        url : urlToFetch,
		crossDomain: true,
        async: false,
		type: 'POST',
		data: $('form[name=logonForm]').serialize(),
		dataType: 'json',
		success: function(data) {
            serverResponse = data.val;
		},
		error: function(aj, desc, errorThrown) {
			//console.log("Error: [" + desc + "] errorThrown ["+errorThrown +"]" );
		},
		statusCode: {
			404: function() {
				//console.log("page not found");
			}
		}
	});
    if (serverResponse != -1) {
       if (serverResponse == 1) {
          return true;
       }
       else {
          return false;
       }
    }
    else {
       return false;
    }
}
doSubmit=function() {
   $('#errMsg').empty();
   if ($('#recaptcha_response_field').val() == "") {
      $('#errMsg').addClass('wrng').empty().html('<td>Por favor ingrese todos los caracteres que usted visualiza arriba.</td>');
      return false;
   }
   var captchaPassed = doCaptchaValidate();
   if (captchaPassed) {
	  clkLgn();
      return true;
   }
   else {
      showRecaptcha('recaptcha_div');
	  $('#errMsg').addClass('wrng').empty().html('<td>Por favor ingrese todos los caracteres que usted visualiza arriba.</td>');
      return false;
   }
}

$(document).ready(function() { setup(); });