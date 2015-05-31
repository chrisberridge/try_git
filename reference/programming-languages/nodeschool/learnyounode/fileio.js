var fs = require('fs');
var fileContents = fs.readFileSync(process.argv[2]).toString().split("\n");
var lines = fileContents.length - 1;
console.log(lines);