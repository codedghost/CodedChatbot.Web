/// <binding ProjectOpened='watch' />
var gulp = require('gulp');
var rename = require('gulp-rename');
var sass = require('gulp-sass');
var postcss = require('gulp-postcss');
var cssnano = require('cssnano');
var autoprefixer = require('autoprefixer');

var paths = {
    styles: './Styles/**/*.scss',
    css: './wwwroot/css/'
}

gulp.task('styles', function () {
    var plugins = [
        autoprefixer(),
        cssnano()
    ];

    return gulp.src(paths.styles)
        .pipe(sass())
        .pipe(postcss(plugins))
        .pipe(rename(function(path) {
            path.basename = path.basename.toLowerCase() + '.min';
        }))
        .pipe(gulp.dest(paths.css));
});

gulp.task('watch', function() {
    gulp.watch(paths.styles, gulp.series('styles'));
});