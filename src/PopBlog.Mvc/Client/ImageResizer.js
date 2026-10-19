// Photos are resized in the browser before upload, so the server never has to decode image data.
// Ported from PopForums' ImageResizer, but each format stays in its own format.
class ImageResizer {
	static maxWidth = 1000;
	static maxHeight = 1000;
	static quality = 0.7;

	static async prepare(file) {
		// canvas can't encode GIF, so it uploads untouched and keeps its animation
		if (file.type === "image/gif")
			return file;
		const isPng = file.type === "image/png";
		const image = await ImageResizer.load(file);
		const width = image.naturalWidth;
		const height = image.naturalHeight;
		const scale = Math.min(1, ImageResizer.maxWidth / width, ImageResizer.maxHeight / height);
		if (isPng && scale === 1)
			return file;

		// JPEGs always get re-encoded, even when small enough, so EXIF data (like GPS location) is
		// stripped and the rotation is baked in.
		const dw = Math.max(1, Math.round(width * scale));
		const dh = Math.max(1, Math.round(height * scale));
		const canvas = ImageResizer.draw(image, dw, dh);
		// only sharpen what was actually scaled down, not a JPEG that was just re-encoded
		if (scale < 1)
			ImageResizer.sharpen(canvas);
		const type = isPng ? "image/png" : "image/jpeg";
		const blob = await ImageResizer.toBlob(canvas, type, ImageResizer.quality);
		const name = file.name.replace(/\.[^.]*$/, "") + (isPng ? ".png" : ".jpg");
		return new File([blob], name, { type: type });
	}

	static load(file) {
		// An img element (unlike createImageBitmap in older Safari) applies EXIF orientation everywhere.
		const url = URL.createObjectURL(file);
		const image = new Image();
		image.src = url;
		return image.decode()
			.then(() => image)
			.catch(() => { throw new Error(ImageResizer.errorMessage); })
			.finally(() => URL.revokeObjectURL(url));
	}

	static draw(image, dw, dh) {
		// Halving in steps looks far better than one big jump down, and the first step being half size
		// keeps huge phone photos under iOS Safari's canvas memory limit.
		let source = image;
		let sw = image.naturalWidth;
		let sh = image.naturalHeight;
		while (sw / 2 >= dw && sh / 2 >= dh) {
			const step = ImageResizer.createCanvas(Math.round(sw / 2), Math.round(sh / 2));
			step.getContext("2d").drawImage(source, 0, 0, sw, sh, 0, 0, step.width, step.height);
			source = step;
			sw = step.width;
			sh = step.height;
		}
		const canvas = ImageResizer.createCanvas(dw, dh);
		canvas.getContext("2d").drawImage(source, 0, 0, sw, sh, 0, 0, dw, dh);
		return canvas;
	}

	// Matches the light GaussianSharpen(0.5) the server used to apply after resizing: a separable
	// unsharp kernel (twice the original minus a Gaussian blur), run along rows and then down columns.
	// Color is sharpened premultiplied by alpha, so transparent PNG pixels don't bleed dark halos.
	static sharpen(canvas) {
		const context = canvas.getContext("2d");
		const { width, height } = canvas;
		const image = context.getImageData(0, 0, width, height);
		const data = image.data;
		const sigma = 0.5;
		// Gaussian weight one pixel from center, relative to center; the two-pixel taps are under 0.1% and dropped
		const side = Math.exp(-1 / (2 * sigma * sigma));
		const edge = -side / (1 + 2 * side);
		const center = 1 - 2 * edge;
		const premultiplied = new Float32Array(data.length);
		for (let i = 0; i < data.length; i += 4) {
			const alpha = data[i + 3] / 255;
			for (let c = 0; c < 3; c++)
				premultiplied[i + c] = data[i + c] * alpha;
			premultiplied[i + 3] = data[i + 3];
		}
		const rows = new Float32Array(data.length);
		for (let y = 0; y < height; y++) {
			for (let x = 0; x < width; x++) {
				const i = (y * width + x) * 4;
				const left = (y * width + Math.max(x - 1, 0)) * 4;
				const right = (y * width + Math.min(x + 1, width - 1)) * 4;
				for (let c = 0; c < 4; c++)
					rows[i + c] = center * premultiplied[i + c] + edge * (premultiplied[left + c] + premultiplied[right + c]);
			}
		}
		for (let y = 0; y < height; y++) {
			for (let x = 0; x < width; x++) {
				const i = (y * width + x) * 4;
				const up = (Math.max(y - 1, 0) * width + x) * 4;
				const down = (Math.min(y + 1, height - 1) * width + x) * 4;
				const alpha = Math.min(255, Math.max(0, center * rows[i + 3] + edge * (rows[up + 3] + rows[down + 3])));
				for (let c = 0; c < 3; c++) {
					const value = center * rows[i + c] + edge * (rows[up + c] + rows[down + c]);
					data[i + c] = alpha > 0 ? value * 255 / alpha : 0;
				}
				data[i + 3] = alpha;
			}
		}
		context.putImageData(image, 0, 0);
	}

	static createCanvas(width, height) {
		const canvas = document.createElement("canvas");
		canvas.width = width;
		canvas.height = height;
		const context = canvas.getContext("2d");
		context.imageSmoothingEnabled = true;
		context.imageSmoothingQuality = "high";
		return canvas;
	}

	static toBlob(canvas, type, quality) {
		return new Promise((resolve, reject) => {
			canvas.toBlob(blob => {
				// null means the browser couldn't allocate the canvas, usually because the image is enormous
				if (blob)
					resolve(blob);
				else
					reject(new Error(ImageResizer.errorMessage));
			}, type, quality);
		});
	}

	static errorMessage = "This image couldn't be processed.";

	// Swaps the chosen file for the resized one when the form is submitted, then submits it normally.
	static attach(form, input, errorElement) {
		form.addEventListener("submit", async (event) => {
			if (!input.files.length)
				return;
			event.preventDefault();
			const button = form.querySelector("[type=submit]");
			button.disabled = true;
			errorElement.textContent = "";
			try {
				const prepared = await ImageResizer.prepare(input.files[0]);
				const transfer = new DataTransfer();
				transfer.items.add(prepared);
				input.files = transfer.files;
				// form.submit() doesn't raise the submit event, so this doesn't loop
				form.submit();
			}
			catch (error) {
				errorElement.textContent = error.message;
				button.disabled = false;
			}
		});
	}
}
