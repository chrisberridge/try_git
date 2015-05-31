
var mymodule = require('./module.js');
mymodule(process.argv[2], process.argv[3], function (err, files) {
  if (err) {
    console.error('There was an error:', err);
  }
  else {
    files.forEach(function(file) {console.log(file);});
  }
});