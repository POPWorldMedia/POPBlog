/// <binding AfterBuild="default" />

var gulp = require("gulp"),
	merge = require("merge-stream"),
	babel = require("gulp-babel"),
	cssmin = require("gulp-cssmin"),
	uglify = require("gulp-uglify"),
	sourcemaps = require("gulp-sourcemaps"),
	rename = require("gulp-rename");

var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("copies", function () {
	var streams = [
		gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "bootstrap/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "tinymce"))
	];
	return merge(streams);
});

gulp.task("js", function () {
	return gulp.src("./Client/*.js")
		.pipe(sourcemaps.init())
		.pipe(babel({ presets: ["@babel/preset-env"], sourceMap: true }))
		.pipe(uglify())
		.pipe(rename({ suffix: '.min' }))
		.pipe(sourcemaps.write("./"))
		.pipe(gulp.dest("./wwwroot/dist"));
});

gulp.task("css", function () {
	return gulp.src("./Client/*.css")
		.pipe(cssmin())
		.pipe(rename({ suffix: '.min' }))
		.pipe(gulp.dest("./wwwroot/dist"));
});

gulp.task("default", gulp.series(["copies", "js", "css"]));
