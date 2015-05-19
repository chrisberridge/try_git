<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EcCines.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Scripts/jquery-ui-1.11.3.min.js"></script>
    <script src="Scripts/jquery-2.1.3.min.js"></script>
    <script src="Scripts/toastr.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
    <link href="css/toastr.css" rel="stylesheet" />
</head>
<body>
    <div id="Content" style="width:260px; margin: 0 auto; padding-top: 100px;">
    <form id="form1" runat="server">
        <div>
                <div class="head">
                    <div class="logo">
                        <img src="images/logo.png" style="margin-left: 10px;" />
                    </div>
                    <div class="title">
                        <label style="width: 150%; margin-left: -71px;">CARTELERA DE CINES EL COLOMBIANO</label>
                        <label class="page" style="width: 117%; margin-left: -45px;">GESTIÓN DE PELICULAS</label>
                    </div>
                </div>
                <hr />
            </div>
         <div style="width:255px; margin: 0 auto;">
    <asp:Panel ID="login" runat="server">
        <div class="ui-widget ui-widget-content ui-corner-all" id="login_admin">
        <table>
            <tr>
                <td>Usuario</td><td><asp:TextBox ID="txtUsuario" runat="server" CssClass="input_login"/></td>
            </tr>
            <tr>
                <td>Contraseña</td><td><asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" CssClass="input_login"/></td>
            </tr>
        </table>
            <asp:Button ID="btnAutenticar" runat="server" Text="Aceptar" OnClick="AutenticarUsuario" CssClass="btn btnHorario" />
        </div>
    </asp:Panel>
             </div>
    </form>
        </div>
</body>
</html>
