<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="exelaunch.aspx.cs" Inherits="EC.SE4Migrate.Web.exelaunch" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>EXE Launcher to migration packager</title>
</head>
<body>
    <form id="frmMigration" runat="server">
        <div>
            <asp:Label runat="server">Comando:</asp:Label>
            <asp:TextBox runat="server" id="txtCommand" name="txtCommand" Width="341px"></asp:TextBox>
        </div>
        <p>
            <asp:Button ID="btnExeLaunch" runat="server" Text="EXE Launcher" OnClick="ExeLaunch_Click" />
            <asp:Label ID="lblInfo" name="lblInfo" runat="server"></asp:Label>
        </p>
    </form>
</body>
</html>
