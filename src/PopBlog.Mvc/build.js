// Front-end asset build: minifies Client/*.js and Client/*.css into wwwroot/dist (with
// sourcemaps), and copies vendored node_modules assets into wwwroot/lib. Must be fully
// self-sufficient and runnable before `dotnet build` (e.g. in CI).
const esbuild = require("esbuild");
const fs = require("fs");
const path = require("path");

const clientPath = "Client";
const distPath = path.join("wwwroot", "dist");
const libPath = path.join("wwwroot", "lib");
const nodeRoot = "node_modules";

function copyDir(srcDir, destDir) {
	fs.mkdirSync(destDir, { recursive: true });
	fs.cpSync(srcDir, destDir, { recursive: true });
}

function minify(fileName, extension, options) {
	esbuild.buildSync({
		entryPoints: [path.join(clientPath, fileName)],
		outfile: path.join(distPath, fileName.replace(new RegExp(`\\${extension}$`), `.min${extension}`)),
		bundle: false,
		minify: true,
		logLevel: "warning",
		...options
	});
}

fs.mkdirSync(distPath, { recursive: true });

for (const fileName of fs.readdirSync(clientPath)) {
	if (fileName.endsWith(".js")) minify(fileName, ".js", { sourcemap: true, target: "es2018" });
	else if (fileName.endsWith(".css")) minify(fileName, ".css");
}

copyDir(path.join(nodeRoot, "bootstrap", "dist"), path.join(libPath, "bootstrap", "dist"));
copyDir(path.join(nodeRoot, "tinymce"), path.join(libPath, "tinymce"));
