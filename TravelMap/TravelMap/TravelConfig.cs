﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.IO;
using Core.Math;
using Core.Shell.Common.FileSystems;
using Newtonsoft.Json;

namespace TravelMap
{
	public class TravelConfig
	{
		public readonly InternalConfig Config;
		public readonly LocationTimeline Locations;
		public readonly PhotoCollection Photos;

		public TravelConfig (string configDirectory = null)
		{
			if (string.IsNullOrWhiteSpace (configDirectory)) {
				configDirectory = @"../../..";
			}
			if (!configDirectory.EndsWith ("/")) {
				configDirectory += "/";
			}

			string configPath = configDirectory + @"config.json";
			string locationPath = configDirectory + @"locations.json";
			string photoPath = configDirectory + @"photos.json";

			Config = ConfigHelper.OpenConfig<InternalConfig> (fullPath: configPath);

			if (Config.PictureSourceDirectories_Internal == null) {
				Config.PictureSourceDirectories_Internal = new List<string> ();
			}

			if (string.IsNullOrWhiteSpace (Config.Html5OutputDirectory_Internal)) {
				Config.Html5OutputDirectory_Internal = null;
			}

			if (Config.UtcOffsets == null || Config.UtcOffsets.Count == 0) {
				Config.UtcOffsets = new List<UtcOffset> (new [] { UtcOffset.DefaultOffset (DateTime.Now) });
			}

			ConfigHelper.SaveConfig (fullPath: configPath, stuff: Config);

			Locations = new LocationTimeline (fullPath: locationPath);

			Photos = new PhotoCollection (fullPath: photoPath);
		}

		public class InternalConfig
		{
			[JsonProperty ("directories_source_pictures")]
			public List<string> PictureSourceDirectories_Internal { get; set; }

			[JsonIgnore]
			public List<VirtualDirectory> PictureSourceDirectories {
				get {
					return PictureSourceDirectories_Internal
						.Select (d => FileSystemSubsystems.ParseNativePath (d) as VirtualDirectory)
						.Where (d => d != null && d.Path.VirtualPath.Length != 0)
						.ToList ();
				}
			}

			[JsonProperty ("directory_output_html5")]
			public string Html5OutputDirectory_Internal { get; set; }

			[JsonIgnore]
			public VirtualDirectory Html5OutputDirectory {
				get {
					VirtualDirectory vd = FileSystemSubsystems.ParseNativePath (Html5OutputDirectory_Internal) as VirtualDirectory;
					if (vd != null && vd.Path.VirtualPath.Length != 0) {
						return vd;
					} else {
						return null;
					}
				}
			}

			[JsonProperty ("directory_thumbnail_cache")]
			public string ThumbnailCacheDirectory_Internal { get; set; }

			[JsonIgnore]
			public VirtualDirectory ThumbnailCacheDirectory {
				get {
					VirtualDirectory vd = FileSystemSubsystems.ParseNativePath (ThumbnailCacheDirectory_Internal) as VirtualDirectory;
					if (vd != null && vd.Path.VirtualPath.Length != 0) {
						return vd;
					} else {
						return null;
					}
				}
			}

			[JsonProperty ("utc_offsets")]
			public List<UtcOffset> UtcOffsets { get; set; }

			public InternalConfig ()
			{
			}
		}
	}
}

