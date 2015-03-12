<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="showmoviewidget.aspx.cs" Inherits="EcCines.showmoviewidget" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Show Movie Widget</title>
    <meta content="text/html; charset=UTF-8" http-equiv="content-type" />

    <link href="css/bootstrap-combined.min.css" rel="stylesheet" type="text/css" media="screen" />
    <link href="css/bootstrap-select.min.css" rel="stylesheet" type="text/css" media="screen" />

    <link href="css/reset.css" rel="stylesheet" type="text/css" media="screen" />    
    <link href="css/planepoly.css" rel="stylesheet" type="text/css" media="screen" />

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
    <script src="http://netdna.bootstrapcdn.com/twitter-bootstrap/2.3.2/js/bootstrap.js"></script>
    <script src="Scripts/bootstrap-select.min.js"></script>
    <script src="Scripts/movieservice.js"></script>
    <script src="Scripts/urlencode.js"></script>
    <link href="css/styles.css" rel="stylesheet" type="text/css" media="screen" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="movieInfo">
            <section class="seccion-cines">
                <div class="buscador-peliculas">
                    <div class="content">
                        <div class="logo-cines">
                        </div>
                        <div class="search">
                            <div id="movieByTheaters" class="input-busqueda teatro"></div>
                            <div id="movieByMovies" class="input-busqueda teatro"></div>
                            <div id="movieByGenre" class="input-busqueda teatro"></div>
                            <div class="clear"></div>
                        </div>
                        <div class="clear"></div>
                    </div>
                </div>
                <div class="peliculas-cartelera" id="errorBox"></div>
                <div class="peliculas-cartelera" id="showMovieList">
                    <div class="content" id="showMovieListData"></div>
                </div>
            </section>
        </div>
    </form>
</body>
</html>
