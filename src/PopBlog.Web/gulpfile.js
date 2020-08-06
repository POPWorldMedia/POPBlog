/// <binding AfterBuild="default" />

var gulp = require("gulp"),
	merge = require("merge-stream"),
	babel = require("gulp-babel"),
	cssmin = require("gulp-cssmin"),
	uglify = require("gulp-uglify"),
	sourcemaps = require("gulp-sourcemaps"),
	rename = require("gulp-rename"),
	bump = require("gulp-bump");

var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("copies", function () {
	var streams = [
		gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "/bootstrap/dist")),
		gulp.src(nodeRoot + "popper.js/dist/umd/popper.min.js").pipe(gulp.dest(targetPath + "/popper.js/dist")),
		gulp.src(nodeRoot + "jquery/dist/**/*").pipe(gulp.dest(targetPath + "/jquery/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "/tinymce"))
	];
	return merge(streams);
});

gulp.task("js", function () {
	return gulp.src("./wwwroot/lib/PopBlog/src/*.js")
		.pipe(sourcemaps.init())
		.pipe(babel({ presets: ["@babel/preset-env"], sourceMap: true }))
		.pipe(uglify())
		.pipe(rename({ suffix: '.min' }))
		.pipe(sourcemaps.write("./"))
		.pipe(gulp.dest(targetPath + "/PopBlog/dist"));
});

gulp.task("css", function () {
	return gulp.src("./wwwroot/lib/PopBlog/src/*.css")
		.pipe(cssmin())
		.pipe(rename({ suffix: '.min' }))
		.pipe(gulp.dest(targetPath + "/PopBlog/dist"));
});

gulp.task("bump", function () {
	return gulp.src("./wwwroot/lib/PopBlog/package.json")
		.pipe(bump({ type: "prerelease" }))
		.pipe(gulp.dest("./wwwroot/lib/PopBlog/"));
});

gulp.task("min", gulp.series(["copies", "js", "css", "bump"]));

gulp.task("default", gulp.series("min"));