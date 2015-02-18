/*
Date:     Feb.16/2014
Modified: Feb.17/2015
*/
var hostBaseSearch = 'http://localhost:56633/';
var hostBaseWidgetMovie = 'http://localhost:56633';
var hostBaseWidgetEvent = 'http://localhost:56633';
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
var idMovieBillboard = '#cartelera-cine-planepoly';

initializeSearchMovies = function () {
    var urlToFetch = hostBaseWidgetMovie + "/ReadJson.aspx?m=2";
    $.support.cors = true; // Required for IE.
    $.ajaxSetup({ scriptCharset: "utf-8", contentType: "application/json; charset=utf-8" });
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
fillMovieList = function () {
    var items = [];
    items.push("<h6>Buscar por película</h6>");
    items.push("<select id='byMovie'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in movieSearchData.movies) {
        items.push("<option value='" + movieSearchData.movies[i].id + "|" +  movieSearchData.movies[i].idFormat + "'>" + movieSearchData.movies[i].nameFull + "</option>");
    }
    items.push("</select>");
    $(idMovieByMovies).empty();
    $(idMovieByMovies).append(items.join(""));
    $(idByMovie).selectpicker();
}
fillMovieByTeatherList = function (theaterSelected) {
    var theaterMovies = movieSearchData.theaterMovies;
    var items = [];
    items.push("<h6>Buscar por película</h6>");
    items.push("<select id='byMovie'>");
    items.push("<option value='-1'>- seleccione -</option>");
    for (var i in theaterMovies[theaterSelected]) {
        items.push("<option value='" + theaterMovies[theaterSelected][i].id + "|" +  theaterMovies[theaterSelected][i].idFormat + "'>" + theaterMovies[theaterSelected][i].nameFull + "</option>");
    }
    items.push("</select>");
    $(idMovieByMovies).empty();
    $(idMovieByMovies).append(items.join(""));
    $(idByMovie).selectpicker();
}
fillGenreList = function () {
    var items = [];
    items.push("<h6>Buscar por genero</h6>");
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
onTheatersChange = function () {
    var theaterSelectedId = $(idTheaters).val();
    var theaterSelectedText = $(idTheaters + ' option:selected').text();

    $(idGenres).val('-1');
    $(idGenres).selectpicker('refresh');
    $(idMovieByMovies).empty();
    $(errorBox).css('display', 'none');
    if (theaterSelectedId != '-1') {
        fillMovieByTeatherList(theaterSelectedText);
    } else {
        fillMovieList();
    }
    $(idByMovie).change(onByMovieChange);
    onMovieSearch();
    if (theaterSelected.length > 24) {
        $("#movieByTheaters .bootstrap-select.btn-group .btn .filter-option").text(theaterSelected.substring(0, 23));
    }
}
onByMovieChange = function () {
    $(idGenres).val('-1');
    $(idGenres).selectpicker('refresh');
    $(errorBox).css('display', 'none');
    onMovieSearch();
    var movieSelected = $(idByMovie).val();

    if (movieSelected.length > 24) {
        $("#movieByMovies .bootstrap-select.btn-group .btn .filter-option").text(movieSelected.substring(0, 23));
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
    var genreSelected = $(idMovieByGenre).val();

    if (genreSelected.length > 24) {
        $("#movieByGenre .bootstrap-select.btn-group .btn .filter-option").text(genreSelected.substring(0, 23));
    }
}
paintMovies = function (data, theaterSelected, byMovieSelected, byGenreSelected) {
    var items = [];
    var j = 1;
    var dataLen = data.length;
    var urlModified = '';
    var startRow = false;

    items.push('<ul id="cartelera-cine-planepoly">');
    for (var i in data) {        
        if (j === 1) {
            items.push('<li>');
            startRow = true;
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
        items.push('<a href="' + urlModified + '" target="_blank">');
        items.push('<img src="' + data[i].img + '" alt="" border="" width="221px" height="309px"/>');
        items.push('</a>');
        items.push('</figure>');
        items.push('</div>');

        if (j > 4) {
            items.push('</li>');
            j = 1;
            startRow = false;
        }
        if (i >= (dataLen - 1)) {
            if (startRow) {
                while (j <= 4) {
                    items.push('<div class="pelicula-empty"></div>');
                    j++;
                }
            }
            items.push('</li>');
        }
    }
    items.push('</ul>');
    return items.join("");
}
paintMoviesFiltered = function (data, theaterSelected, byMovieSelected, byGenreSelected) {
    console.log('Filtered');
    var items = [];
    var j = 1;
    var dataLen = data.length;
    var now = new Date();
    var day = now.getDay();

    if (day == 0) {
        //In javascript Sunday is 0, but in our system it is 7
        day = 7;
    }
    items.push('<ul id="cartelera-cine-planepoly">');
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
        items.push('<a href="' + urlModified + '" target="_blank">');
        items.push('<img src="' + data[i].img + '" alt="" border="" width="221px" height="309px"/>');
        items.push('</a>');
        items.push('</figure>');
        items.push('</div>');

        if (j > 4) {
            items.push('</li>');
            j = 1;
        }
        if (i >= (dataLen - 1)) {
            if (j <= 4) {
                while (j <= 4) {
                    items.push('<div class="pelicula-empty"></div>');
                    j++;
                }
            }
            items.push('</li>');
        }
    }
    items.push('</ul>');
    return items.join("");
}
onMovieSearch = function () {
    var theaterSelected = $(idTheaters).val();
    var byMovieSelected = $(idByMovie).val();
    var byGenreSelected = $(idGenres).val();
    var urlToFetch = hostBaseWidgetMovie + "/ecmoviesearch.aspx";
    var parameterList = "t=" + urlencode(theaterSelected) + "&m=" + urlencode(byMovieSelected) + "&g=" + urlencode(byGenreSelected);

    urlToFetch += "?" + parameterList;
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
                $(errorBox).append("<span>No se encontraron resultados</span>");
                $(idShowMovieList).css('display', 'none');
                $(errorBox).css('display', 'block');
            } else {
                var output;

                $(idShowMovieList).css('display', 'block');
                if (theaterSelected == "-1" && byMovieSelected == "-1" && byGenreSelected == "-1") {
                    output = paintMovies(data, theaterSelected, byMovieSelected, byGenreSelected);
                } else {
                    output = paintMoviesFiltered(data, theaterSelected, byMovieSelected, byGenreSelected);
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
                //console.log("page not found");
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