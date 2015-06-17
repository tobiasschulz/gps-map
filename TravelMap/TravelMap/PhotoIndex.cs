using System;
using System.Collections.Generic;
using Core.Common;
using Core.Math;
using Core.Shell.Common.FileSystems;
using TravelMap.Pictures;

namespace TravelMap
{
	public class PhotoIndex
	{
		readonly TravelConfig config;
		readonly Exif exif = new Exif ();

		public PhotoIndex (TravelConfig config)
		{
			this.config = config;
		}

		public void SyncFromFiles ()
		{
			List<VirtualDirectory> sources = config.Config.PictureSourceDirectories;

			if (sources == null || sources.Count == 0) {
				Log.Error ("Error: PictureSourceDirectories is an empty list!");
				return;
			}

			Log.Info ("Collect Photos:");
			Log.Indent++;

			Log.Info ("all sources: ", config.Config.PictureSourceDirectories.Join (", "));

			foreach (VirtualDirectory source in sources) {
				SyncFromDirectory (source: source);
			}

			Log.Indent--;
		}

		void SyncFromDirectory (VirtualDirectory source)
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
					if (photo.DateTime.HasValue) {
						photo.DateTimeUTC = photo.DateTime.Value + UtcOffset.FindOffset (list: config.Config.UtcOffsets, dateTimeLocal: photo.DateTime.Value).ToTimeSpan ();
						photo.Location = config.Locations.InterpolateLocation (photo.DateTimeUTC.Value);
					}

					Log.Info (file.Path.FileName, ": ", photo.Location);

					config.Photos.AddPhoto (photo);
				}
			}

			Log.Indent--;
		}
	}
}

