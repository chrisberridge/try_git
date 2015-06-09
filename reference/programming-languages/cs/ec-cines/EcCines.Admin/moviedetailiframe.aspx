<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="moviedetailiframe.aspx.cs" Inherits="EcCines.Admin.moviedetailiframe" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Movie Detail FRAME</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
    <script src="Scripts/iframeResizer.min.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
    <link rel="shortcut icon" href="./images/ec-logo.jpeg" />
</head>
<body>
    <div id="Content">
        <form id="form1" runat="server">
            <asp:Panel runat="server" ID="divHTMLApoyo">
                <div>
                    <label for="HTML_apoyo">Insertar código HTML_apoyo</label>
                    <asp:TextBox ID="htmlApoyo" runat="server" TextMode="MultiLine" MaxLength="1024" Width="300px" Height="100px"></asp:TextBox>
                </div>
            </asp:Panel>
            <asp:HiddenField runat="server" ID="idPelicula" />
            <iframe id="movieDetailFrame" src="" width="100%" frameborder="0" scrolling="no"></iframe>                        
        </form>        
    </div>
</body>
<script language="javascript">
    $(document).ready(function () {
        var a = window.location.search.substring(1);
        var url = window.location.origin;
        if (url == "undefined")
            url = window.location.protocol + "//" + window.location.host;
        $("#movieDetailFrame").attr("src", url + "/showmoviedetail.aspx?" + a);
        $('#movieDetailFrame').iFrameResize({
            log: true,
            enablePublicMethods: true,
            enableInPageLinks: false,
            resizedCallback: function (messageData) { 
                $('p#callback').html(
                    '<b>Frame ID:</b> ' + messageData.iframe.id +
                    ' <b>Height:</b> ' + messageData.height +
                    ' <b>Width:</b> ' + messageData.width +
                    ' <b>Event type:</b> ' + messageData.type
                );
            },
            messageCallback: function (messageData) {
                $('p#callback').html(
                    '<b>Frame ID:</b> ' + messageData.iframe.id +
                    ' <b>Message:</b> ' + messageData.message
                );
                alert(messageData.message);
            },
            closedCallback: function (id) { // Callback fn when iFrame is closed
                $('p#callback').html(
                    '<b>IFrame (</b>' + id +
                    '<b>) removed from page.</b>'
                );
            }
        });
    });

</script>
</html>
