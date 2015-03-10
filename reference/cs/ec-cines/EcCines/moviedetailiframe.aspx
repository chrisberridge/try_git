<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="moviedetailiframe.aspx.cs" Inherits="EcCines.moviedetailiframe" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Movie Detail FRAME</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
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
            <iframe id="movieDetailFrame" src="" width="655px" height="500px" frameborder="0"></iframe>
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
    });

</script>
</html>
