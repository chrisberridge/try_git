<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Horario.aspx.cs" Inherits="EcCines.Admin.Horario" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Horarios</title>
    <script src="Scripts/jquery-2.1.3.min.js"></script>
    <script src="Scripts/jquery-ui-1.11.3.min.js"></script>
    <script src="Scripts/horario.js?v=20150415"></script>
    <script src="Scripts/jquery.maskedinput-1.4.1.js"></script>
    <link href="css/jquery-ui.min.css" rel="stylesheet" />    
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
                        <label class="title_head page">HORARIOS</label>
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
                <hr />
                <span>Seleccione la película a la que asignará el horario, e ingrese las horas por cada formato para cada fecha.</span>
            </div>            
            <asp:ScriptManager runat="server" ID="smUpdate"></asp:ScriptManager>
            <asp:Button ID="btnGuardarTop" runat="server" Text="Guardar y volver" OnClick="OnButtonGuardar" CssClass="btn btnHorario" />
            <asp:UpdatePanel runat="server" ID="upPrincipal">
                <ContentTemplate>
                    <div>
                        <div id="sala_cine" class="title">
                            Sala de Cine:&nbsp;
                                <asp:Label ID="lblTeatroSeleccionado" runat="server" Text="" CssClass="titulo_sala"></asp:Label>
                        </div>
                        <div id="fecha_ppal">
                            <div class="addFecha" id="addFecha">
                                <span class="addFecha">Adicionar Fecha</span><img id="imgFecha" title="Adicionar Fecha" src="images/MasFecha.jpg" />
                            </div>
                        </div>
                        <div class="clear"></div>
                        <div class="divDerecho" id="Derecho">
                            <asp:ListBox ID="ListBoxPeliculas" runat="server" Height="297px" Width="393px" AutoPostBack="true" OnSelectedIndexChanged="ObtenerProgramacion" onchange="ObtenerInfoProgramacion()"></asp:ListBox>
                        </div>
                        <div class="divIzquierdo title" id="izquierdo">
                            <div id="contentFechas">
                                <asp:Literal ID="ltFechas" runat="server" />
                            </div>
                        </div>
                    </div>
                    <asp:HiddenField ID="teatroSeleccionado" runat="server" />
                    <asp:HiddenField ID="infoProgramacion" runat="server" />
                    <asp:HiddenField ID="codNuevaFecha" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar y volver" OnClick="OnButtonGuardar" CssClass="btn btnHorario" />
            <asp:Button ID="btnDisableEnter" runat="server" Text="" OnClientClick="return false;" style="display:none;"/>
        </form>
    </div>
</body>
</html>
