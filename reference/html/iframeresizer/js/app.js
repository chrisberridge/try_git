 $(document).ready(function() {
     $("#divHv").html("<iframe id='frameHv' width='100%' height='2500px' frameborder='0' scrolling='no'></iframe>");
     $("#frameHv").attr("src", "framecontent.html");
     $("#frameHv").iFrameResize({
         log: true, // Enable console logging
         enablePublicMethods: true, // Enable methods within iframe hosted page
         enableInPageLinks: true,
         
         resizedCallback: function(messageData) { // Callback fn when resize is received
             $('p#callback').html('<b>Frame ID:</b> ' + messageData.iframe.id + ' <b>Height:</b> ' + messageData.height + ' <b>Width:</b> ' + messageData.width + ' <b>Event type:</b> ' + messageData.type);
         },
         messageCallback: function(messageData) { // Callback fn when message is received
             $('p#callback').html('<b>Frame ID:</b> ' + messageData.iframe.id + ' <b>Message:</b> ' + messageData.message);
             alert(messageData.message);
         },
         closedCallback: function(id) { // Callback fn when iFrame is closed
             $('p#callback').html('<b>IFrame (</b>' + id + '<b>) removed from page.</b>');
         }
     });
 });