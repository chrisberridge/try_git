<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="EcCines.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Peliculas</title>
    <script src="Scripts/jquery-2.1.3.min.js"></script>
    <script src="Scripts/Index.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
</head>
<body>
    <div id="Content">
        <form id="form1" runat="server">
            <section>
                <div>
                    <div class="head">
                        <div class="logo">
                            <img src="images/logo.png" />
                        </div>
                        <div class="title">
                            <label class="title_head">CARTELERA DE CINES EL COLOMBIANO</label>
                            <label class="title_head page">GESTIÓN DE PELICULAS</label>
                        </div>
                    </div>
                    <hr />
                    <asp:Button PostBackUrl="CinesHorarios.aspx" ID="btnCinesHorarios" runat="server" Text="Cines y horarios" CssClass="btn cines" />
                    <asp:Button ID="btnActualizarCartelara" runat="server" Text="Actualizar Cartelera" OnClick="OnButtonActualizarCartelera" CssClass="btn" />
                    <asp:Button PostBackUrl="entidad.aspx" ID="btnAdminMaestros" runat="server" Text="Admin Parámetros" CssClass="btn" />
                    <asp:Button PostBackUrl="teatro.aspx" ID="btnAdminTeatros" runat="server" Text="Admin Salas de cines" CssClass="btn" />
                    <asp:Button PostBackUrl="cine.aspx" ID="btnAdminCines" runat="server" Text="Admin Cines" CssClass="btn" />
                </div>
                <br class="clear" />
                <div class="msg">
                    <hr>
                    <span>Ingrese la información de una nueva película o seleccione una existente para editarla.</span>
                </div>
                <div>
                    <div class="title">
                        <label for="nombre" class="">Nombre de película: (*)</label>
                        <asp:TextBox ID="nombre" runat="server" CssClass="camp-text"></asp:TextBox>
                    </div>
                    <div class="title">
                        <label for="url">URL Detalle Milenium:</label>
                        <asp:TextBox ID="url" runat="server" CssClass="camp-text"></asp:TextBox>
                    </div>
                    <div class="title">
                        <label for="imgUpload">Imagen:</label>
                        <asp:Image ID="imgPoster" runat="server" />
                        <asp:FileUpload ID="imgUpload" runat="server" />
                    </div>
                    <div class="title">
                        <label for="listGenero">Genero: (*)</label>
                        <asp:DropDownList ID="listGenero" runat="server"></asp:DropDownList>
                    </div>
                    <div class="title">
                        <label for="checkEnCartelera">En cartelera?</label>
                        <asp:CheckBox ID="checkEnCartelera" runat="server" Text="" />
                    </div>
                    <div class="title">
                        <label for="sinopsis">Sinopsis:</label>
                        <asp:TextBox ID="sinopsis" runat="server" TextMode="MultiLine" MaxLength="1024" Width="96.5%" Height="100px"></asp:TextBox>
                    </div>
                    <asp:HiddenField runat="server" ID="idPelicula" />
                    <asp:Button ID="btnVistaPrevia" runat="server" Text="Vista previa" CssClass="btn" />
                    <asp:Button ID="btnNuevo" runat="server" OnClick="OnButtonNuevo" Text="Guardar" CssClass="btn" />
                    <asp:Button ID="btnActualizar" runat="server" OnClick="OnButtonActualizar" Text="Actualizar" CssClass="btn" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="OnButtonCancelar" CssClass="btn" />
                    <br />
                    <br />
                    <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>
                    <asp:Label ID="lblError" runat="server" Text="" CssClass="error"></asp:Label>
                </div>
                <br />
            </section>
            <section>
                <div>
                    <hr />
                    <div class="msg">
                        <span>Filtrar por nombre de película</span>
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="camp-text" />
                        <asp:Button ID="ButtonLimpiar" runat="server" Text="Limpiar" CssClass="btn" OnClick="OnButtonLimpiar" />
                        <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn" OnClick="OnButtonFiltrar" />
                    </div>
                    <asp:GridView ID="grdInfo" runat="server" AllowPaging="True" AutoGenerateColumns="False" CellPadding="4" DataKeyNames="idPelicula" ForeColor="#333333"
                        GridLines="None" OnSelectedIndexChanged="OnGridInfoSelectedIndexChanged" OnPageIndexChanging="OnGridInfoPageIndexChanging" CssClass="mGrid">
                        <AlternatingRowStyle BackColor="White" />
                        <Columns>
                            <asp:BoundField DataField="idPelicula" HeaderText="Id" SortExpression="idPelicula" />
                            <asp:BoundField DataField="nombrePelicula" HeaderText="Nombre Película" SortExpression="nombrePelicula" />
                            <asp:BoundField DataField="fechaCreacionPelicula" HeaderText="Creación" SortExpression="fechaCreacionPelicula" />
                            <asp:BoundField DataField="urlArticuloEC" HeaderText="URL" SortExpression="urlArticuloEC" />
                            <asp:BoundField DataField="enCartelera" HeaderText="En Cartelera" SortExpression="enCartelera" />
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
            </section>
        </form>
    </div>
</body>
</html>
