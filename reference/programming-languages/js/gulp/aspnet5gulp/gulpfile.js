/*
  This file in the main entry point for defining Gulp tasks and using Gulp plugins.
  Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
'use strict';
var gulp = require('gulp'),
    tsc = require('gulp-tsc'),
    shell = require('gulp-shell'),
    seq = require('run-sequence'),
    del = require('del');
var paths = {
    ts: {
        src: ['scripts/ts/*.ts'],
        dest: 'scripts'
    }
};
// Default
gulp.task('default', ['build']);
// Clean
gulp.task('clean', function(cb) {
    del(paths.ts.dest + '/*.js', cb);
});
// ReBuild - Clean & Build
gulp.task('rebuild', function(cb) {
    seq('clean', 'build', cb);
});
// Build
gulp.task('build', function() {
    return gulp.src(paths.ts.src).pipe(tsc({
        module: "CommonJS",
        sourcemap: true,
        emitError: false
    })).pipe(gulp.dest(paths.ts.dest));
});
gulp.task('watch', function() {
    gulp.watch(paths.ts.src, ['build']);
});