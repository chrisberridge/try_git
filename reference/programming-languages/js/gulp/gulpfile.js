var gulp = require('gulp');
var sass = require('gulp-sass');
var browserSync = require('browser-sync');
var useref = require('gulp-useref');
var uglify = require('gulp-uglify');
var gulpIf = require('gulp-if');
var minifyCSS = require('gulp-minify-css');
var imagemin = require('gulp-imagemin');
var cache = require('gulp-cache');
var del = require('del');
var runSequence = require('run-sequence');
gulp.task('sass', function() {
    return gulp.src('app/scss/**/*.scss').pipe(sass()) // Converts Sass to CSS with gulp-sass
    .pipe(gulp.dest('app/css')).pipe(browserSync.reload({
        stream: true
    }));
});
gulp.task('browserSync', function() {
    browserSync({
        server: {
            baseDir: 'app'
        },
    })
});
gulp.task('watch', ['browserSync', 'sass'], function() {
    gulp.watch('app/scss/**/*.scss', ['sass']);
    // Reloads the browser whenever HTML or JS files change
    gulp.watch('app/*.html', browserSync.reload);
    gulp.watch('app/js/**/*.js', browserSync.reload);
});
gulp.task('useref', function() {
    var assets = useref.assets();
    return gulp.src('app/*.html').pipe(assets)
    // Minifies only if it's a CSS file
    .pipe(gulpIf('*.css', minifyCSS()))
    // Uglifies only if it's a Javascript file
    .pipe(gulpIf('*.js', uglify())).pipe(assets.restore()).pipe(useref()).pipe(gulp.dest('dist'))
});
gulp.task('images', function() {
    return gulp.src('app/images/**/*.+(png|jpg|gif|svg)').pipe(cache(imagemin({
        // Setting interlaced to true
        interlaced: true
    }))).pipe(gulp.dest('dist/images'))
});
gulp.task('fonts', function() {
    return gulp.src('app/fonts/**/*').pipe(gulp.dest('dist/fonts'))
});
gulp.task('clean', function() {
    del('dist');
    return cache.clearAll(callback);
});
gulp.task('clean:dist', function(callback) {
    del(['dist/**/*', '!dist/images', '!dist/images/**/*'], callback)
});
gulp.task('build', function(callback) {
    runSequence('clean:dist', ['sass', 'useref', 'images', 'fonts'], callback)
});
gulp.task('default', function(callback) {
    runSequence(['sass', 'browserSync', 'watch'], callback)
});