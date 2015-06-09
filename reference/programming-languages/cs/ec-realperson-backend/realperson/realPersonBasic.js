var hostBase = "http://localhost:62670";

doCheck=function() {
    var urlToFetch = hostBase + "/realperson.aspx";
    var serverResponse = -1;
	$.support.cors = true; // Required for IE.
	$.ajax({
        url : urlToFetch,
		crossDomain: true,
        async: false,
		data: $('form[name=logonForm]').serialize(),
		type: 'POST',
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
   var captchaPassed = doCheck();
   if (captchaPassed) {
      document.forms[0].action = "http://localhost:62670/realperson.aspx";
      return true;
   }
   else {
      alert("Debe digitar las letras");
      return false;
   }
}