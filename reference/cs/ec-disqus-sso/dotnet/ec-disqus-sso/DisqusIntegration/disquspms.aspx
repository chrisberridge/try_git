<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="disquspms.aspx.cs" Inherits="Ec.Disqus.Web.Integration.disquspms" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
      <asp:Label ID="lblUserId" runat="server">Id:</asp:Label><asp:TextBox id="userId" runat="server" /><br />
      <asp:Label ID="lblUserName" runat="server">User Name:</asp:Label><asp:TextBox id="userName" runat="server" /><br />
      <asp:Label ID="lblEmail" runat="server">Email:</asp:Label><asp:TextBox id="email" runat="server" /><br />
      <asp:Label ID="lblAvatar" runat="server">Avatar:</asp:Label><asp:TextBox id="avatar" runat="server" /><br />
      <asp:Button ID="btnSend" runat="server" Text="Enviar"/>
    <div>
    <label id="lblData" runat="server"></label>
    </div>
    </form>
</body>
</html>
