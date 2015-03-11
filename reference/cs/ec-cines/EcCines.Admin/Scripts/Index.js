windowOpen = function (movieId) {
    var url = window.location.origin + "/moviedetailiframe.aspx?m=" + movieId;
    window.open(url, "VistaPreviaPelicula");    
    return false;
}
$(document).ready(function () {
    $('#btnVistaPrevia').click(function (event) {
        var movieId = $('#idPelicula').val();
        if (movieId != "0") {
            windowOpen(movieId);
        }        
        event.preventDefault();
    });
});