<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="teatro.aspx.cs" Inherits="EcCines.teatro" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Salas de Cine</title>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
</head>
<body>
    <div id="Content">
        <form id="frmMaestroTeatro" runat="server">
            <div>
                <div class="head">
                    <div class="logo">
                        <img src="images/logo.png" />
                    </div>
                    <div class="title">
                        <label class="title_head">CARTELERA DE CINES EL COLOMBIANO</label>
                        <label class="title_head page">GESTIÓN DE SALAS DE CINE</label>
                    </div>
                </div>
                <hr />
                <asp:Button PostBackUrl="Index.aspx" ID="btnHome" runat="server" Text="Películas" CssClass="btn cines" />
                <asp:Button PostBackUrl="CinesHorarios.aspx" ID="btnCinesHorarios" runat="server" Text="Cines y horarios" CssClass="btn cines" />
                <asp:Button PostBackUrl="entidad.aspx" ID="btnAdminMaestros" runat="server" Text="Admin Parámetros" CssClass="btn" />
                <asp:Button PostBackUrl="cine.aspx" ID="btnAdminCines" runat="server" Text="Admin Cines" CssClass="btn" />
            </div>
            <br class="clear" />
            <div class="msg">
                <hr>
            </div>
            <div class="title">
                <p>
                    <span>Cine: (*) </span>
                    <asp:DropDownList ID="listaCines" runat="server"></asp:DropDownList>
                </p>
                <p>
                    <span>Nombre: (*)</span>
                    <asp:TextBox ID="txtNombre" runat="server" CssClass="camp-text"></asp:TextBox>
                </p>
                <p>
                    <span>Telefono 1: </span>
                    <asp:TextBox ID="txtTelefono1" runat="server" CssClass="camp-text"></asp:TextBox>
                </p>
                <p>
                    <span>Telefono 2: </span>
                    <asp:TextBox ID="txtTelefono2" runat="server" CssClass="camp-text"></asp:TextBox>
                </p>
                <p>
                    <span>Telefono 3: </span>
                    <asp:TextBox ID="txtTelefono3" runat="server" CssClass="camp-text"></asp:TextBox>
                </p>
                <p>
                    <span>Municipio: (*)</span>
                    <asp:DropDownList ID="listaMunicipios" runat="server"></asp:DropDownList>
                </p>
                <p>
                    <span>Departamento: (*)</span>
                    <asp:DropDownList ID="listaDepartamentos" runat="server"></asp:DropDownList>
                </p>
                <p>
                    <span>Dirección: (*)</span>
                    <asp:TextBox ID="txtDireccion" runat="server" CssClass="camp-text"></asp:TextBox>
                </p>
            </div>
            <div>
                <asp:Button ID="btnNuevo" runat="server" Text="Guardar" OnClick="OnButtonNuevo" CssClass="btn" />
                <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" OnClick="OnButtonEliminar" CssClass="btn" />
                <asp:Button ID="btnActualizar" runat="server" Text="Actualizar" OnClick="OnButtonActualizar" CssClass="btn" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="OnButtonCancelar" CssClass="btn" />
                <br />
                <br />
                <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>
                <asp:Label ID="lblError" runat="server" Text="" CssClass="error"></asp:Label>
                <br />
                <br />
            </div>
            <div>
                <asp:GridView ID="grdInfo" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="idTeatro"
                    ForeColor="#333333"
                    GridLines="None" OnSelectedIndexChanged="OnGridInfoSelectedIndexChanged" OnPageIndexChanging="OnGridInfoPageIndexChanging">
                    <AlternatingRowStyle BackColor="White" />
                    <Columns>
                        <asp:BoundField DataField="nombreCine" HeaderText="Cine" SortExpression="nombreCine" />
                        <asp:BoundField DataField="nombreTeatro" HeaderText="Teatro" SortExpression="nombreTeatro" />
                        <asp:BoundField DataField="telefono1Teatro" HeaderText="Telefono 1" SortExpression="telefono1Teatro" />
                        <asp:BoundField DataField="telefono2Teatro" HeaderText="Telefono 2" SortExpression="telefono2Teatro" />
                        <asp:BoundField DataField="telefono3Teatro" HeaderText="Telefono 3" SortExpression="telefono3Teatro" />
                        <asp:BoundField DataField="nombreMunicipio" HeaderText="Municipio" SortExpression="nombreMunicipio" />
                        <asp:BoundField DataField="nombreDepartamento" HeaderText="Departamento" SortExpression="nombreDepartamento" />
                        <asp:BoundField DataField="direccionTeatro" HeaderText="Dirección" SortExpression="direccionTeatro" />
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
