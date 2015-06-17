﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using Core.Common;
using System.Drawing.Drawing2D;

namespace TravelMap.Pictures
{
	public static class Resize
	{
		static readonly Exif exif = new Exif ();

		public static ImageCodecInfo GetEncoder (ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders ();
			foreach (ImageCodecInfo codec in codecs) {
				if (codec.FormatID == format.Guid) {
					return codec;
				}
			}
			return null;
		}

		public static Bitmap ResizeImage (Image sourceImage, int maxWidth, int maxHeight)
		{
			// Determine which ratio is greater, the width or height, and use
			// this to calculate the new width and height. Effectually constrains
			// the proportions of the resized image to the proportions of the original.
			double xRatio = (double)sourceImage.Width / maxWidth;
			double yRatio = (double)sourceImage.Height / maxHeight;
			double ratioToResizeImage = Math.Max (xRatio, yRatio);
			int newWidth = (int)Math.Floor (sourceImage.Width / ratioToResizeImage);
			int newHeight = (int)Math.Floor (sourceImage.Height / ratioToResizeImage);

			// Create new image canvas -- use maxWidth and maxHeight in this function call if you wish
			// to set the exact dimensions of the output image.
			Bitmap newImage = new Bitmap (newWidth, newHeight, PixelFormat.Format32bppArgb);

			// Render the new image, using a graphic object
			using (Graphics newGraphic = Graphics.FromImage (newImage)) {
				// Set the background color to be transparent (can change this to any color)
				newGraphic.Clear (Color.Transparent);

				// Set the method of scaling to use -- HighQualityBicubic is said to have the best quality
				newGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

				// Apply the transformation onto the new graphic
				Rectangle sourceDimensions = new Rectangle (0, 0, sourceImage.Width, sourceImage.Height);
				Rectangle destinationDimensions = new Rectangle (0, 0, newWidth, newHeight);
				newGraphic.DrawImage (sourceImage, destinationDimensions, sourceDimensions, GraphicsUnit.Pixel);
			}

			// Image has been modified by all the references to it's related graphic above. Return changes.
			return newImage;
		}

		public static bool ResizeImage (string sourcePath, string destPath, string mimeType, int maxWidth, int maxHeight, int quality)
		{
			try {
				Image original = Image.FromFile (sourcePath);
				Bitmap resized = ResizeImage (sourceImage: original, maxWidth: maxWidth, maxHeight: maxHeight);
				if (mimeType == "image/jpeg") {
					//EncoderParameters encoderParams = new EncoderParameters (1);
					//encoderParams.Param [0] = new EncoderParameter (System.Drawing.Imaging.Encoder.Quality, quality);
					//resized.Save (filename: destPath, encoder: GetEncoder (ImageFormat.Jpeg), encoderParams: encoderParams);
					BitMiracle.LibJpeg.JpegHelper.Current.Save (image: resized, filename: destPath, compression: new BitMiracle.LibJpeg.CompressionParameters { Quality = quality });

					exif.CopyExifTags (sourcePath: sourcePath, destPath: destPath);
				} else if (mimeType == "image/png") {
					resized.Save (filename: destPath, format: ImageFormat.Png);
					exif.CopyExifTags (sourcePath: sourcePath, destPath: destPath);
				} else {
					throw new ArgumentException ("Invalid mimetype for ResizeImage: mimeType=" + mimeType + ", sourcePath=" + sourcePath);
				}
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
	}
}

