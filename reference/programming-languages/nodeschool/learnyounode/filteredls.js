var fs = require('fs');
var path = require('path');
fs.readdir(process.argv[2], function(err, files){
	for (var index = 0; index < files.length; index++) {
		var element = files[index];
		if (path.extname(element) === '.' + process.argv[3]) {
		    console.log(element);	
		}
	}
});