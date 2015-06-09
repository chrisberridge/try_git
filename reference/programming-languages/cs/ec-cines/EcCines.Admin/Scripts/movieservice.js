/*
Date:     Feb.16/2014
Modified: Apr.15/2015
*/
var hostBaseSearch = 'http://localhost:8001/';
var hostBaseWidgetMovie = hostBaseSearch;
var hostBaseWidgetEvent = hostBaseSearch;
var movieSearchData;
var idMovieByTheaters = '#movieByTheaters';
var idMovieByMovies = '#movieByMovies';
var idMovieByGenre = '#movieByGenre';
var idTheaters = '#theaters';
var idByMovie = '#byMovie';
var idGenres = '#genres';
var idShowMovieList = '#showMovieList';
var idErrorBox = '#errorBox';
var idShowAllMovies = '#showAllMovies';
var idShowMovieListData = '#showMovieListData';
var idMovieSearch = '#movieSearch';
var idMovieBillboard = '#cartelera-cine';

initializeSearchMovies = function () {
    var urlToFetch = hostBaseWidgetMovie + "/ReadJson.aspx?m=2";
    $.support.cors = true; // Required for IE.
    $.ajaxSetup({ scriptCharset: "utf-8", contentType: "application/json; charset=utf-8", crossDomain: true });
    $.ajax({
        url: urlToFetch,
        crossDomain: true,
        async: false,
        type: 'POST',
        dataType: 'json',
        success: function (data) {
            movieSearchData = data;
        },
        error: function (aj, desc, errorThrown) {
            //console.log("Error: [" + desc + "] errorThrown ["+errorThrown +"]" );
        },
        statusCode: {
            404: function () {
                //console.log("page not found");
            }
        }
    });
}
fillTheaterList = function () {
    var items = [];
    items.push("<h6>Buscar por teatro</h6>");
    items.push("<select id='theaters'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in movieSearchData.theaters) {
        items.push("<option value='" + movieSearchData.theaters[i].id + "'>" + movieSearchData.theaters[i].nameFull + "</option>");
    }
    items.push("</select>");
    $(idMovieByTheaters).empty();
    $(idMovieByTheaters).append(items.join(""));
    $(idTheaters).selectpicker();
}
fillTheaterByMovieList = function () {
    var movieSelectedId = $(idByMovie).val();
    var aMovieID = movieSelectedId.split("|");
    var movieInTheaters = movieSearchData.movieInTheaters[aMovieID[0]];
    var items = [];
    items.push("<h6>Buscar por teatro</h6>");
    items.push("<select id='theaters'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in movieInTheaters) {        
        items.push("<option value='" + movieInTheaters[i].id + "'>" + movieInTheaters[i].nameFull + "</option>");
    }
    items.push("</select>");    
    $(idMovieByTheaters).empty();
    $(idMovieByTheaters).append(items.join(""));
    $(idTheaters).selectpicker();
}
fillMovieList = function () {
    var items = [];
    items.push("<h6>Buscar por película</h6>");
    items.push("<select id='byMovie'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in movieSearchData.movies) {
        for (var j in movieSearchData.movies[i].formats) {
            var movieId = movieSearchData.movies[i].id;
            var formatId = movieSearchData.movies[i].formats[j].id;
            var movieName = movieSearchData.movies[i].name;
            var formatName = movieSearchData.movies[i].formats[j].name;
            var key = movieId + "|" + formatId;
            var value = movieName + " " + formatName;
            items.push("<option value='" + key + "'>" + value + "</option>");
        }
    }
    items.push("</select>");
    $(idMovieByMovies).empty();
    $(idMovieByMovies).append(items.join(""));
    $(idByMovie).selectpicker();
}
fillMovieByTeatherList = function () {
    var items = [];
    var theaterSelectedText = $(idTheaters + ' option:selected').text();
    items.push("<h6>Buscar por película</h6>");
    items.push("<select id='byMovie'>");
    items.push("<option value='-1'>- seleccione -</option>");
    var movieList = movieSearchData.theaterMovies[theaterSelectedText];
    for (var i in movieList) {
        for (var j in movieList[i].formats) {
            var movieId = movieList[i].id;
            var formatId = movieList[i].formats[j].id;
            var movieName = movieList[i].name;
            var formatName = movieList[i].formats[j].name;
            var key = movieId + "|" + formatId;
            var value = movieName + " " + formatName;
            items.push("<option value='" + key + "'>" + value + "</option>");
        }
    }
    items.push("</select>");
    $(idMovieByMovies).empty();
    $(idMovieByMovies).append(items.join(""));
    $(idByMovie).selectpicker();
}
fillGenreList = function () {
    var items = [];
    items.push("<h6>Buscar por género</h6>");
    items.push("<select id='genres'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in movieSearchData.genres) {
        items.push("<option value='" + movieSearchData.genres[i].id + "'>" + movieSearchData.genres[i].name + "</option>");
    }
    items.push("</select>");
    $(idMovieByGenre).empty();
    $(idMovieByGenre).append(items.join(""));
    $(idGenres).selectpicker();
}
paintMovies = function (data, theaterSelected, byMovieSelected, byGenreSelected) {
    var items = [];
    var j = 1;
    var dataLen = data.length;
    var urlModified = '';

    items.push('<ul id="cartelera-cine">');
    for (var i in data) {
        if (j === 1) {
            items.push('<li>');
        }
        j++;

        urlModified = data[i].url;
        urlModified += '?m=' + data[i].id;
        if (byMovieSelected != '-1') {
            urlModified += '&f=' + data[i].idFormat;
        }
        if (theaterSelected != '-1') {
            urlModified += '&t=' + theaterSelected
        }
        items.push('<div class="pelicula">');
        items.push('<figure class="poster-pelicula">');

        var isPremiere;

        if ($.isArray(data)) {
            isPremiere = data[i].premiere;
        } else {
            isPremiere = data.premiere;
        }
        if (isPremiere == 'S') {
            items.push('<div class="new-movie"><img border="0" src="' + hostBaseSearch + '/images/titular-estreno.png"></div>');
        }
        items.push('<a href="' + urlModified + '" target="_blank">');
        items.push('<img src="' + data[i].img + '" alt="" border="0" width="221px" height="309px"/>');
        items.push('</a>');
        items.push('</figure>');
        items.push('</div>');

        if (j > 4) {
            items.push('</li>');
            j = 1;
        }
    }
    if ((dataLen % 4) != 0) {
        var numElemsLeftToDraw = 4 - (dataLen % 4);
        j = 1;
        while (j <= numElemsLeftToDraw) {
            items.push('<div class="pelicula"><figure class="poster-pelicula"></figure></div>');
            j++;
        }
        items.push('</li>');
    }
    items.push('</ul>');
    return items.join("");
}
paintMovieOnly = function (data, theaterSelected, byMovieSelected, byGenreSelected) {
    var items = [];
    var j = 1;
    var dataLen = 1;
    var urlModified = '';

    items.push('<ul id="cartelera-cine">');
    items.push('<li>');
    urlModified = data.url;
    urlModified += '?m=' + data.id;
    if (byMovieSelected != '-1') {
        var aMovieID = byMovieSelected.split("|");
        urlModified += '&f=' + aMovieID[1];
    }
    if (theaterSelected != '-1') {
        urlModified += '&t=' + theaterSelected
    }
    items.push('<div class="pelicula">');
    items.push('<figure class="poster-pelicula">');

    var isPremiere;

    if ($.isArray(data)) {
        isPremiere = data[i].premiere;
    } else {
        isPremiere = data.premiere;
    }
    if (isPremiere == 'S') {
        items.push('<div class="new-movie"><img border="0" src="' + hostBaseSearch + '/images/titular-estreno.png"></div>');
    }
    items.push('<a href="' + urlModified + '" target="_blank">');
    items.push('<img src="' + data.img + '" alt="" border="0" width="221px" height="309px"/>');
    items.push('</a>');
    items.push('</figure>');
    items.push('</div>');
    items.push('</li>');
    if ((dataLen % 4) != 0) {
        var numElemsLeftToDraw = 4 - (dataLen % 4);
        j = 1;
        items.push('<li>');
        while (j <= numElemsLeftToDraw) {
            items.push('<div class="pelicula"><figure class="poster-pelicula"></figure></div>');
            j++;
        }
        items.push('</li>');
    }
    items.push('</ul>');
    return items.join("");
}
onTheatersChange = function () {
    var theaterSelectedId = $(idTheaters).val();
    var theaterSelectedText = $(idTheaters + ' option:selected').text();

    $(idGenres).val('-1');
    $(idGenres).selectpicker('refresh');
    $(idMovieByMovies).empty();
    $(errorBox).css('display', 'none');
    if (theaterSelectedId == '-1') {
        fillMovieList();
    } else {
        fillMovieByTeatherList();
    }
    $(idByMovie).change(onByMovieChange);
    onMovieSearch();
    if (theaterSelectedText.length > 22) {
        $("#movieByTheaters .bootstrap-select.btn-group .btn .filter-option").text(theaterSelectedText.substring(0, 21));
    }
}
onByMovieChange = function () {
    var movieSelectedId = $(idByMovie).val();
    var movieSelectedText = $(idByMovie + ' option:selected').text();
    $(idGenres).val('-1');
    $(idGenres).selectpicker('refresh');   
    $(errorBox).css('display', 'none');
    onMovieSearch();
    if (movieSelectedText.length > 22) {
        $("#movieByMovies .bootstrap-select.btn-group .btn .filter-option").text(movieSelectedText.substring(0, 21));
    }
}
onGenreChange = function () {
    var byMovieList;
    var theaterMovies;

    $(errorBox).css('display', 'none');
    $(idTheaters).val("-1");
    $(idTheaters).selectpicker('refresh');
    $(idByMovie).val("-1");
    fillMovieList();
    $(idByMovie).change(onByMovieChange);
    onMovieSearch();
    var genreSelected = $(idMovieByGenre + ' option:selected').text();

    if (genreSelected.length > 22) {
        $("#movieByGenre .bootstrap-select.btn-group .btn .filter-option").text(genreSelected.substring(0, 21));
    }
}
onMovieSearch = function () {
    var theaterSelected = $(idTheaters).val();
    var byMovieSelected = $(idByMovie).val();
    var byGenreSelected = $(idGenres).val();
    var urlToFetch = hostBaseWidgetMovie + "/ecmoviesearch.aspx";

    if (byMovieSelected === undefined) {
        byMovieSelected = "-1";
    }
    urlToFetch += "?" + "t=" + urlencode(theaterSelected) + "&m=" + urlencode(byMovieSelected) + "&g=" + urlencode(byGenreSelected);
    $(errorBox).css('display', 'none');
    $.support.cors = true; // Required for IE.
    $.ajaxSetup({ scriptCharset: "utf-8", contentType: "application/json; charset=utf-8" });
    $.ajax({
        url: urlToFetch,
        crossDomain: true,
        type: 'POST',
        dataType: 'json',
        success: function (data) {
            if (data != null && data.length == 0) {
                $(errorBox).empty();
                $(errorBox).append("<div class='content'><span>No se encontraron resultados</span></div>");
                $(idShowMovieList).css('display', 'none');
                $(errorBox).css('display', 'block');
            } else {
                var output;

                $(idShowMovieList).css('display', 'block');
                if (theaterSelected == "-1" && byMovieSelected == "-1" && byGenreSelected == "-1") {
                    output = paintMovies(data, theaterSelected, byMovieSelected, byGenreSelected);
                } else {
                    if (byMovieSelected != "-1") {
                        output = paintMovieOnly(data, theaterSelected, byMovieSelected, byGenreSelected);
                    } else {
                        output = paintMovies(data, theaterSelected, byMovieSelected, byGenreSelected);
                    }
                }
                $(idShowMovieListData).empty();
                $(idShowMovieListData).append(output);
            }
        },
        error: function (aj, desc, errorThrown) {
            //console.log("Error: [" + desc + "] errorThrown ["+errorThrown +"]" );
        },
        statusCode: {
            404: function () {
                //  console.log("page not found");
            }
        }
    });
}
loadMovieSearchData = function () {
    if (movieSearchData != null) {
        fillTheaterList();
        fillMovieList();
        fillGenreList();
        $(idTheaters).change(onTheatersChange);
        $(idByMovie).change(onByMovieChange);
        $(idGenres).change(onGenreChange);
        onMovieSearch();
    }
}
$(function () {
    initializeSearchMovies();
    loadMovieSearchData();
});