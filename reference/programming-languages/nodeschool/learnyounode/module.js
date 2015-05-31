var fs = require('fs');
var path = require('path');

module.exports = function (dir, ext, f) {
	fs.readdir(dir, function (err, files) {
		if (err) {
			f(err);
		} else {
			var filteredFiles = [];
			files.forEach(function (file) {
				if (path.extname(file) === '.' + ext) {
					filteredFiles.push(file);
				}
			});
			f(null, filteredFiles);
		}
	});
};