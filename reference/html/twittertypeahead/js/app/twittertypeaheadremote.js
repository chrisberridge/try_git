/// <reference path="../../typings/jquery/jquery.d.ts"/>
/* global Bloodhound */

// Get your data source
var remoteUrl = 'http://localhost:8080/ServicioJSONWeb.aspx?m=5&q=%QUERYSTRING';
var dataSource = new Bloodhound({
    datumTokenizer: Bloodhound.tokenizers.obj.whitespace('nombres'),
    queryTokenizer: Bloodhound.tokenizers.whitespace,
    remote: {
        url: remoteUrl,
        wildcard: '%QUERYSTRING'
    }
});


$(document).ready(function () {
    // fire a select event, what you want once a user has selected an item
    $('input.countries').typeahead({
        hint: true,
        highlight: true,
        minLength: 1
    },
        {
            display: 'nombres',
            source: dataSource
        }).on('typeahead:opened', onOpened)
        .on('typeahead:selected', onAutocompleted)
        .on('typeahead:autocompleted', onSelected)
        .on('typeahead:change', onChanged)
        .on('typeahead:cursorchange', onCursorChange);


});

var onOpened = function ($e) {
    console.log('opened');
};

var onAutocompleted = function ($e, datum) {
    console.log('onAutocompleted');
    console.log(datum);
};

var onSelected=function($e, datum) {
    console.log('onSelected');
    console.log(datum);
};

var onChanged=function($e, datum) {
    console.log('onChanged');
    console.log(datum);
};

var  onCursorChange=function($e, datum) {
    console.log('onCursorChange');
    console.log(datum);
};