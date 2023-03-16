var gulp = require('gulp'),
    del = require('del'),
    imagemin = require('gulp-imagemin'),
    concat = require('gulp-concat'),
    rename = require('gulp-rename'),
    uglify = require('gulp-uglify'),
    util = require("gulp-util"),
    runSequence = require('run-sequence'),
    cleanCSS = require('gulp-clean-css'),
    sass = require('gulp-sass');

gulp.task('clean', function () {
    return del([
        'Themes/TeedMaterial/Content/css/*',
        'Themes/TeedMaterial/Content/js/*',
        'Themes/TeedMaterial/Content/*'
    ]).then(function () { util.log('clean done...'); });
});

gulp.task("css-aldora", function () {
    return gulp.src([
        'src/styles/main-aldora.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-indigo", function () {
    return gulp.src([
        'src/styles/main-indigo.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-chabelo", function () {
    return gulp.src([
        'src/styles/main-chabelo.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-energiahumana", function () {
    return gulp.src([
        'src/styles/main-energiahumana.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-montecito38", function () {
    return gulp.src([
        'src/styles/main-montecito38.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-logoshop", function () {
    return gulp.src([
        'src/styles/main-logoshop.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-teedcommerce", function () {
    return gulp.src([
        'src/styles/main-teedcommerce.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-bastards", function () {
    return gulp.src([
        'src/styles/main-bastards.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-uvasymoras", function () {
    return gulp.src([
        'src/styles/main-uvasymoras.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-kromtek", function () {
    return gulp.src([
        'src/styles/main-kromtek.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-dermalomas", function () {
    return gulp.src([
        'src/styles/main-dermalomas.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-tecahomestore", function () {
    return gulp.src([
        'src/styles/main-tecahomestore.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-cuchurrumin", function () {
    return gulp.src([
        'src/styles/main-cuchurrumin.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-centralenlinea", function () {
    return gulp.src([
        'src/styles/main-centralenlinea.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterialV2/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterialV2/Content/css'));
});

gulp.task("css-hamleys", function () {
    return gulp.src([
        'src/styles/main-hamleys.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-lining", function () {
    return gulp.src([
        'src/styles/main-lining.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-scalpers", function () {
    return gulp.src([
        'src/styles/main-scalpers.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-inkstudio", function () {
    return gulp.src([
        'src/styles/main-inkstudio.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-emedemar", function () {
    return gulp.src([
        'src/styles/main-emedemar.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedEmeDeMar/Content/css' }))
        .pipe(gulp.dest('Themes/TeedEmeDeMar/Content/css'));
});

gulp.task("css-zanaalquimia", function () {
    return gulp.src([
        'src/styles/main-zanaalquimia.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedZanaAlquimia/Content/css' }))
        .pipe(gulp.dest('Themes/TeedZanaAlquimia/Content/css'));
});

gulp.task("css-masa", function () {
    return gulp.src([
        'src/styles/main-masa.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMasa/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMasa/Content/css'));
});


gulp.task("css-lamy", function () {
    return gulp.src([
        'src/styles/main-lamy.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedLamy/Content/css' }))
        .pipe(gulp.dest('Themes/TeedLamy/Content/css'));
});

gulp.task("css-elpomito", function () {
    return gulp.src([
        'src/styles/main-elpomito.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-lithographic", function () {
    return gulp.src([
        'src/styles/main-lithographic.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-atomica", function () {
    return gulp.src([
        'src/styles/main-atomica.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-pelletier", function () {
    return gulp.src([
        'src/styles/main-pelletier.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-fragoza", function () {
    return gulp.src([
        'src/styles/main-fragoza.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("css-cetro", function () {
    return gulp.src([
        'src/styles/main-cetro.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedCetro/Content/css' }))
        .pipe(gulp.dest('Themes/TeedCetro/Content/css'));
});

gulp.task("css-bububaby", function () {
    return gulp.src([
        'src/styles/main-bububaby.scss'
    ])
        .pipe(sass())
        .pipe(rename('site.min.css'))
        .pipe(cleanCSS({ level: { 1: { specialComments: 0 } }, rebase: true, rebaseTo: 'Themes/TeedMaterial/Content/css' }))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/css'));
});

gulp.task("js", function () {
    return gulp.src([
        'wwwroot/js/gridify.js',
        'Themes/TeedMaterialV2/Content/js/jquery.min.js',
        'node_modules/jquery-validation/dist/jquery.validate.js',
        'node_modules/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
        'node_modules/materialize-css/dist/js/materialize.js',
        'node_modules/knockout/build/output/knockout-latest.js',
        'src/scripts/main.js',
        'src/scripts/default-passive-events.js',
        //'Themes/TeedMaterialV2/Content/js/custom.js',
        'Themes/TeedMaterialV2/Content/js/public.js',
        //'Themes/TeedMaterialV2/Content/js/ui-choose/ui-choose.js',
        'src/scripts/lazysizes.min.js',
        //'src/scripts/font-awesome-5.13.1.js',
        'wwwroot/slick/slick.js',
        //'src/scripts/public.common.js',
        //'src/scripts/public.ajaxcart.js'
    ])
        .pipe(concat('site.min.js', { newLine: '' }))
        .pipe(uglify())
        .pipe(gulp.dest('Themes/TeedMaterialV2/Content/js'));
});

gulp.task('img', function () {
    return gulp.src([
        'src/images/*'
    ])
        .pipe(imagemin())
        .pipe(gulp.dest('Themes/TeedMaterial/Content/images'));
});

gulp.task('extrajs', function () {
    return gulp.src([
        'src/scripts/public.common.js',
        'src/scripts/public.ajaxcart.js'
    ])
        .pipe(concat('public.js'))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/js'));
});

gulp.task('extrajs-hamleys', function () {
    return gulp.src([
        'src/scripts/public.common.js',
        'src/scripts/public.ajaxcart.js'
    ])
        .pipe(concat('public.js'))
        .pipe(gulp.dest('Themes/TeedMaterial/Content/js'));
});

gulp.task('extrajs-TeedMaterialV2', function () {
    return gulp.src([
        'src/scripts/cel.public.common.js',
        'src/scripts/cel.public.ajaxcart.js'
    ])
        .pipe(concat('public.js'))
        .pipe(gulp.dest('Themes/TeedMaterialV2/Content/js'));
});

gulp.task('build', function () {
    runSequence('clean', 'css', 'js', 'img');
});
