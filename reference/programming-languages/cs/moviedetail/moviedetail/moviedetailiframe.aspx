<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="moviedetailiframe.aspx.cs" Inherits="moviedetail.moviedetailiframe" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Movie Detail FRAME</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">        
    <div>
     <iframe id="movieDetailFrame" src="" width="700px" height="500px" frameborder="0"></iframe> 
    </div>
    </form>
</body>
<script language="javascript">
    $(document).ready(function () {
        var a = window.location.search.substring(1);
        $("#movieDetailFrame").attr("src", "http://localhost:56633/showmoviedetail.aspx?" + a);
        });
    
</script>
</html>
