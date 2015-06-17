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

			Config = ConfigHelper.OpenConfig<InternalConfig> (fullPath: configPath);

			if (Config.PictureSourceDirectory_Internal == null) {
				Config.PictureSourceDirectory_Internal = new List<string> ();
			}

			if (Config.UtcOffsets == null || Config.UtcOffsets.Count == 0) {
				Config.UtcOffsets = new List<UtcOffset> (new [] {
					new UtcOffset {
						Start = DateTime.Now.AddYears (-1),
						end = DateTime.Now.AddYears (+1),
						offset = (int)TimeZoneInfo.Local.GetUtcOffset (DateTime.Now).TotalHours
					}
				});
			}

			ConfigHelper.SaveConfig (fullPath: configPath, stuff: Config);

			Locations = new LocationTimeline (fullPath: locationPath);
		}

		public class InternalConfig
		{
			[JsonProperty ("directory_source_pictures")]
			public List<string> PictureSourceDirectory_Internal { get; set; }

			[JsonIgnore]
			public List<VirtualDirectory> PictureSourceDirectory {
				get {
					return PictureSourceDirectory_Internal
						.Select (d => FileSystemSubsystems.ParseNativePath (d) as VirtualDirectory)
						.Where (d => d != null && d.Path.VirtualPath.Length != 0)
						.ToList ();
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

