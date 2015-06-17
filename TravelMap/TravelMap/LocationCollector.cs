﻿using System;
using System.Collections.Generic;
using Core.Common;
using Core.Math;
using Core.Shell.Common.FileSystems;

namespace TravelMap
{
	public class LocationCollector
	{
		readonly TravelConfig config;
		readonly Exif exif = new Exif ();

		public LocationCollector (TravelConfig config)
		{
			this.config = config;
		}

		public void SyncFromFiles ()
		{
			List<VirtualDirectory> sources = config.Config.PictureSourceDirectory;

			if (sources == null || sources.Count == 0) {
				Log.Error ("Error: PictureSourceDirectory is empty!");
				return;
			}

			Log.Info ("Collect locations:");
			Log.Indent++;

			Log.Info ("all sources: ", config.Config.PictureSourceDirectory.Join (", "));

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

				// skip action cams, they don't have gps
				if (file.Path.FileName.StartsWith ("2015_"))
					continue;

				Log.Info ("contains(", file.Path.FileName, "): ", config.Locations.Contains (where: l => l.ReferenceFile == file.Path.FileName));

				if (!config.Locations.Contains (where: l => l.ReferenceFile == file.Path.FileName)) {

					PortableLocation location = exif.GetLocation (file);
					Log.Info (file.Path.FileName, ": ", location?.Latitude, " ", location?.Longitude);

					config.Locations.AddLocation (location);
				}
			}

			Log.Indent--;
		}
	}
}

