<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CinesHorarios.aspx.cs" Inherits="EcCines.Admin.CinesHorarios" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sala de cine y películas</title>
    <script src="Scripts/jquery-2.1.3.min.js"></script>
    <script src="Scripts/toastr.min.js"></script>
    <script src="Scripts/global.js"></script>
    <link href="css/toastr.min.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
    <link rel="shortcut icon" href="./images/ec-logo.jpeg" />
</head>
<body>
    <div id="Content">
        <form id="form1" runat="server" defaultbutton="btnDisableEnter">
            <div>
                <div class="head">
                    <div class="logo">
                        <img src="images/logo.png" />
                    </div>
                    <div class="title">
                        <label class="title_head">CARTELERA DE CINES EL COLOMBIANO</label>
                        <label class="title_head page">SALA DE CINE Y PELÍCULAS</label>
                    </div>
                </div>
                <hr />
                <asp:Button PostBackUrl="Index.aspx" ID="btnCinesHorarios" runat="server" Text="Películas" CssClass="btn cines" />
                <asp:Button PostBackUrl="entidad.aspx" ID="btnAdminMaestros" runat="server" Text="Admin Parámetros" CssClass="btn" />
                <asp:Button PostBackUrl="teatro.aspx" ID="btnAdminTeatros" runat="server" Text="Admin Salas de cines" CssClass="btn" />
                <asp:Button PostBackUrl="cine.aspx" ID="btnAdminCines" runat="server" Text="Admin Cines" CssClass="btn" />
            </div>
            <br class="clear" />
            <div class="msg">
                <hr>
                <span>Seleccione la sala de cine a la cual le asignará las peliculas en cartelera para configurar sus horarios.</span>
            </div>
            <asp:ScriptManager runat="server" ID="smUpdate"></asp:ScriptManager>
            <asp:UpdatePanel runat="server" ID="upPrincipal">
                <ContentTemplate>
                    <div class="divDerecho" id="Derecho">
                        <div class="title">
                            <span class="titulo_sala">Salas de cine</span>
                        </div>
                        <asp:ListBox ID="lbTeatros" runat="server" Height="297px" Width="393px" AutoPostBack="true" OnSelectedIndexChanged="lbTeatros_SelectedIndexChanged"></asp:ListBox>
                    </div>
                    <div class="divIzquierdo" id="izquierdo">
                        <div class="title">
                            <span>Películas</span>
                        </div>
                        <div style="width:398px; height:297px; overflow:auto;">
                            <asp:CheckBoxList ID="checkBoxPeliculas" runat="server" TextAlign="Right" AutoPostBack="true" OnSelectedIndexChanged="checkBoxPeliculas_SelectedIndexChanged"></asp:CheckBoxList>
                        </div>                        
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="guardarContinuar" runat="server" OnClick="submitGuardar" Text="Guardar y continuar" CssClass="btn btnHorario" />
            <asp:Button ID="btnDisableEnter" runat="server" Text="" OnClientClick="return false;" style="display:none;"/>
        </form>
    </div>
</body>
</html>
