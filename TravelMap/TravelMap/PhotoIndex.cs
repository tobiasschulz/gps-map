using System;
using System.Collections.Generic;
using Core.Common;
using Core.IO;
using Core.Math;
using Core.Shell.Common.FileSystems;
using Core.Shell.Platform.FileSystems;
using TravelMap.Hosting;
using TravelMap.Pictures;
using System.Linq;
using System.Drawing;

namespace TravelMap
{
	public class PhotoIndex
	{
		readonly TravelConfig config;
		readonly Exif exif = new Exif ();
		readonly PhotoHosting hoster;

		public PhotoIndex (TravelConfig config, PhotoHosting hoster)
		{
			this.config = config;
			this.hoster = hoster;
		}

		public void SyncFromFiles ()
		{
			List<VirtualDirectory> sources = config.Config.PictureSourceDirectories;
			RegularDirectory thumbnailCache = config.Config.ThumbnailCacheDirectory as RegularDirectory;

			if (sources == null || sources.Count == 0) {
				Log.Error ("Error: PictureSourceDirectories is an empty list!");
				return;
			}
			if (thumbnailCache == null) {
				Log.Error ("Error: No valid ThumbnailCacheDirectory!");
				return;
			}

			thumbnailCache.CreateDirectories ();

			Log.Info ("Collect Photos:");
			Log.Indent++;

			Log.Info ("all sources: ", config.Config.PictureSourceDirectories.Join (", "));

			foreach (VirtualDirectory source in sources) {
				SyncFromDirectory (source: source, thumbnailCache: thumbnailCache);
			}

			Log.Indent--;
		}

		void SyncFromDirectory (VirtualDirectory source, RegularDirectory thumbnailCache)
		{
			Log.Info ("source: ", source);
			Log.Indent++;

			VirtualDirectoryListing listing = source.OpenList ();
			foreach (VirtualFile file in listing.ListFiles()) {
				
				if (!config.Photos.ContainsPhoto (where: l => l.Filename == file.Path.FileName)) {

					PhotoCollection.Photo photo = new PhotoCollection.Photo {
						Filename = file.Path.FileName,
						DateTime = exif.GetExifDate (file),
					};
					if (!FileHelper.Instance.Exists (PathHelper.CombinePath (thumbnailCache.Path.RealPath, file.Path.FileName))) {
						thumbnailCache.GetChildFile (file.Path.FileName).OpenWriter ().WriteBytes (PhotoCollection.CreateThumbnail (file as RegularFile));
					}
					if (photo.DateTime.HasValue) {
						photo.DateTimeUTC = photo.DateTime.Value - UtcOffset.FindOffset (list: config.Config.UtcOffsets, dateTimeLocal: photo.DateTime.Value).ToTimeSpan ();
						photo.Location = config.Locations.InterpolateLocation (photo.DateTimeUTC.Value);
					}
					Image img = Image.FromFile (file.Path.RealPath);
					photo.Dimensions = new PhotoCollection.Photo.PhotoDimensions { Width = img.Width, Height = img.Height };

					Log.Info (file.Path.FileName, ": ", photo.Location);

					config.Photos.AddPhoto (photo);
				}
			}
			foreach (VirtualFile file in listing.ListFiles().OrderBy(f => FileHelper.Instance.Length(f.Path.RealPath))) {

				PhotoCollection.Photo photo = config.Photos.Photos.Photos.First (l => l.Filename == file.Path.FileName);

				if (photo != null) {
					if (string.IsNullOrWhiteSpace (photo.HostedURL) && photo.Filename.Contains ("PANO")) {
						hoster.Host (photo, file);
						config.Photos.Save ();
					}
				}
			}


			Log.Indent--;
		}
	}
}

