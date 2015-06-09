<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logs.aspx.cs" Inherits="EC.SE4Migrate.Web.logs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Logs</title>
</head>
<body>
    <form id="frmLogs" runat="server">
        <div>
            <span style="color: red" id="ErrorMessage" runat="server" />
            <span class="SampleTitle"><b>Check the boxes to select the files, set a password if you like,
      then click the button to zip them up.</b></span>
            <br />
            <br />
            Password:
            <asp:TextBox ID="tbPassword" Password='true' Text="" AutoPostBack runat="server" />
            <span style="color: Red">(Optional)</span>
            <br />
            <br />
            <asp:Button id="btnGo" Text="Zip checked files" AutoPostBack OnClick="btnGoClick" runat="server"/>
            <asp:ListView ID="FileListView" runat="server">

                <LayoutTemplate>
                    <table>
                        <tr id="itemPlaceholder" runat="server" />
                    </table>
                </LayoutTemplate>

                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:CheckBox ID="include" runat="server" /></td>
                        <td>
                            <asp:Label ID="label" runat="server" Text="<%# Container.DataItem %>" /></td>
                    </tr>
                </ItemTemplate>

                <EmptyDataTemplate>
                    <div>Nothing to see here...</div>
                </EmptyDataTemplate>

            </asp:ListView>
        </div>
    </form>
</body>
</html>
