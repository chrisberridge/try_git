<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cine.aspx.cs" Inherits="EcCines.cine" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Cines</title>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
</head>
<body>
    <div id="Content">
        <form id="frmMaestroCine" runat="server">
            <div>
                <div class="head">
                    <div class="logo">
                        <img src="images/logo.png" />
                    </div>
                    <div class="title">
                        <label class="title_head">CARTELERA DE CINES EL COLOMBIANO</label>
                        <label class="title_head page">GESTIÓN DE CINES</label>
                    </div>
                </div>
                <hr />
                <asp:Button PostBackUrl="Index.aspx" ID="btnHome" runat="server" Text="Películas" CssClass="btn cines" />
                <asp:Button PostBackUrl="CinesHorarios.aspx" ID="btnCinesHorarios" runat="server" Text="Cines y horarios" CssClass="btn cines" />                
                <asp:Button PostBackUrl="entidad.aspx" ID="btnAdminMaestros" runat="server" Text="Admin Parámetros" CssClass="btn" />
                <asp:Button PostBackUrl="teatro.aspx" ID="btnAdminTeatros" runat="server" Text="Admin Salas de cines" CssClass="btn" />                
            </div>
            <br class="clear" />
            <div class="msg">
                <hr>
            </div>
            <div>
                <div class="title">
                    <span>Nombre: (*)</span>
                    <asp:TextBox ID="txtNombre" runat="server"></asp:TextBox>
                </div>
                <div class="title">
                    <span>Nit: (*)</span>
                    <asp:TextBox ID="txtNit" runat="server"></asp:TextBox>
                </div>
            </div>
            <div>
                <asp:Button ID="btnNuevo" runat="server" Text="Guardar" OnClick="OnButtonNuevo" CssClass="btn"/>
                <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" OnClick="OnButtonEliminar" CssClass="btn"/>
                <asp:Button ID="btnActualizar" runat="server" Text="Actualizar" OnClick="OnButtonActualizar" CssClass="btn"/>
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="OnButtonCancelar" CssClass="btn"/>
                <br />
                <br />
                <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>
                <asp:Label ID="lblError" runat="server" Text="" CssClass="error"></asp:Label>
                <br />
                <br />
            </div>
            <div>
                <asp:GridView ID="grdInfo" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="idCine" ForeColor="#333333"
                    GridLines="None" OnSelectedIndexChanged="OnGridInfoSelectedIndexChanged" OnPageIndexChanging="OnGridInfoPageIndexChanging">
                    <AlternatingRowStyle BackColor="White" />
                    <Columns>
                        <asp:BoundField DataField="nombreCine" HeaderText="Nombre" SortExpression="nombreCine" />
                        <asp:BoundField DataField="Nit" HeaderText="Nit" SortExpression="Nit" />
                        <asp:CommandField ShowSelectButton="True" />
                    </Columns>
                    <EditRowStyle BackColor="#2461BF" />
                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="#EFF3FB" />
                    <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                    <SortedAscendingCellStyle BackColor="#F5F7FB" />
                    <SortedAscendingHeaderStyle BackColor="#6D95E1" />
                    <SortedDescendingCellStyle BackColor="#E9EBEF" />
                    <SortedDescendingHeaderStyle BackColor="#4870BE" />
                </asp:GridView>
            </div>
        </form>
    </div>
</body>
</html>
