﻿using System;
using System.Drawing;
using BitMiracle.LibJpeg;
using Core.Shell.Platform.FileSystems;
using Core.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Core.Math;

namespace TravelMap
{
	public class PhotoCollection
	{
		readonly PhotoList photos;
		readonly string fullPath;

		public PhotoCollection (string fullPath)
		{
			this.fullPath = fullPath;
			this.photos = ConfigHelper.OpenConfig<PhotoList> (fullPath: fullPath);
		}

		public PhotoCollection ()
		{
		}

		public bool ContainsPhoto (Func<Photo, bool> where)
		{
			return photos.Photos.Any (where);
		}

		public void AddPhoto (Photo photo)
		{
			if (photo != null) {
				if (!photos.Photos.Contains (photo)) {
					photos.AddPhoto (photo);
					Save ();
				}
			}
		}

		public static void CreateThumbnail (RegularFile source, RegularFile destination)
		{
			Bitmap original = (Bitmap)Image.FromFile (source.Path.RealPath);

			JpegHelper.Current.Save (image: original, filename: destination.Path.RealPath, compression: new CompressionParameters { Quality = 50 });
		}

		void Save ()
		{
			ConfigHelper.SaveConfig (fullPath: fullPath, stuff: photos);
		}

		public class PhotoList
		{
			[JsonProperty ("photos")]
			Photo[] Photos_Internal { get { return Photos?.OrderBy (l => l.DateTime)?.ToArray (); } set { Photos = new HashSet<Photo> (value); } }

			[JsonIgnore]
			public HashSet<Photo> Photos { get; set; } = new HashSet<Photo> ();

			public PhotoList ()
			{
			}

			public void AddPhoto (Photo photo)
			{
				Photos.Add (photo);
			}
		}

		public class Photo
		{
			[JsonProperty ("filename")]
			public string Filename { get; set; } = "";

			[JsonProperty ("timestamp_local")]
			public DateTime? DateTime { get; set; } = null;

			[JsonProperty ("timestamp_utc")]
			public DateTime? DateTimeUTC { get; set; } = null;

			[JsonProperty ("location")]
			public PortableLocation Location { get; set; } = null;

			public override string ToString ()
			{
				return string.Format ("[Photo: Filename={0}, DateTime={1}, Location={2}]", Filename, DateTime, Location);
			}

			// Analysis disable once MemberHidesStaticFromOuterClass
			public override bool Equals (object obj)
			{
				Photo other = obj as Photo;
				if (other != null) {
					return Filename == other.Filename
					&& DateTime == other.DateTime
					&& Location == other.Location;
				} else {
					return false;
				}
			}

			public override int GetHashCode ()
			{
				return Filename.GetHashCode ();
			}
		}
	}
}
