<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="showmoviedetail.aspx.cs" Inherits="EcCines.Admin.showmoviedetail" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Ver Detalle Pelicula</title>
    <script src="Scripts/iframeResizer.contentWindow.min.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
    <link rel="shortcut icon" href="./images/ec-logo.jpeg" />
</head>
<body>
    <form id="frmMovieDetail" runat="server">
        <asp:HiddenField runat="server" ID="movieID" />
        <div id="modulo-cine">
            <div class="wrap">
                <div class="name-mc">
                    <p>
                        <asp:Label ID="movieName" runat="server" Text=""></asp:Label>
                    </p>
                </div>
                <div class="select-mc clearfix">
                    <p class="teatro">
                        <span>teatro</span>
                        <asp:DropDownList ID="movieTheater" runat="server" OnSelectedIndexChanged="OnMovieTheaterChanged" AutoPostBack="True"></asp:DropDownList>
                    </p>
                    <p class="formato">
                        <span>formato de la pelicula</span>
                        <asp:DropDownList ID="movieFormat" runat="server" OnSelectedIndexChanged="OnMovieFormatChanged" AutoPostBack="True"></asp:DropDownList>
                    </p>
                </div>
                <asp:Literal runat="server" ID="noscheduleAtAll"></asp:Literal>
                <asp:Literal runat="server" ID="movieAllData"></asp:Literal>
            </div>
        </div>
    </form>
</body>
</html>
