<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="config.aspx.cs" Inherits="EC.SE4Migrate.Web.config" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Config Editor</title>
    <link rel="stylesheet" href="../css/codemirror.css" />
    <link rel="stylesheet" href="../css/monokai.css" />
    <script type="text/javascript" src="../js/codemirror.js"></script>
    <script type="text/javascript" src="../js/loadmode.js"></script>
    <style type="text/css">
        .CodeMirror {
            border: 1px solid #eee;
        }

        .CodeMirror-scroll {
            height: 500px;
            width: 100%;
        }
    </style>
</head>
<body>
    <form id="frmConfig" runat="server">
        <div>
            <asp:Label runat="server">Archivo:</asp:Label><asp:Button ID="btnSave" runat="server" Text="Guardar" OnClick="Save_Click" />
        </div>
        <p>
            <asp:TextBox ID="txtConfigFile" runat="server" TextMode="MultiLine" ForeColor="#4C667F"
                BackColor="#f2f2f2" Style="margin-top: 10px;"></asp:TextBox>
        </p>
    </form>
    <script type="text/javascript">

        function endsWith(str, suffix) {
            return str.indexOf(suffix, str.length - suffix.length) !== -1;
        }

        CodeMirror.modeURL = "../js/%N.js";

        var editor = CodeMirror.fromTextArea(document.getElementById("txtConfigFile"), {
            lineNumbers: true, theme:"eclipse"
        });

        
        var mode = null;
        mode = "javascript";
        editor.setOption("mode", mode);
        CodeMirror.autoLoadMode(editor, mode);

        editor.refresh();
    </script>
</body>
</html>
