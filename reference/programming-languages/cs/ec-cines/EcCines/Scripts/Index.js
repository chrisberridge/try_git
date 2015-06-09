/*
Date:     Feb.11/2014
Modified: Apr.08/2015
*/
windowOpen = function (movieId) {
    var url = window.location.origin;
    if (url == "undefined") {
        url = window.location.protocol + "//" + window.location.host;
    }
    url = url + "/admin/moviedetailiframe.aspx?m=" + movieId;
    window.open(url, "VistaPreviaPelicula");
    return false;
}
bindEvents = function () {
    $('#btnVistaPrevia').on('click', function (e) {
        var movieId = $('#idPelicula').val();
        if (movieId != "0") {
            windowOpen(movieId);
        }
        e.preventDefault();
    });
}
$(document).ready(function () {    
    bindEvents();
});