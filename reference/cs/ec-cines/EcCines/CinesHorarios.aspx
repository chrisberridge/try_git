<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CinesHorarios.aspx.cs" Inherits="EcCines.CinesHorarios" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sala de cine y películas</title>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
</head>
<body>
    <div id="Content">
        <form id="form1" runat="server">
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
                        <asp:ListBox ID="ListBox2" runat="server" Height="297px" Width="393px" AutoPostBack="true" OnSelectedIndexChanged="ListBox2_SelectedIndexChanged"></asp:ListBox>
                    </div>
                    <div class="divIzquierdo" id="izquierdo">
                        <div class="title">
                            <span>Películas</span>                            
                        </div>
                        <div>
                            <asp:CheckBoxList ID="checkBoxPeliculas" runat="server" Height="289px" Width="386px" TextAlign="Right" AutoPostBack="true" OnSelectedIndexChanged="checkBoxPeliculas_SelectedIndexChanged"></asp:CheckBoxList>
                        </div>
                        <asp:Button ID="guardarContinuar" runat="server" OnClick="submitGuardar" Text="Guardar y continuar" CssClass="btn btnHorario"/>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </form>
    </div>
</body>
</html>
