using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BiasedBit.MinusEngine;
using Core.Common;
using Core.Shell.Common.FileSystems;

namespace TravelMap.Hosting
{
	public class MinusHosting : PhotoHosting
	{
		readonly MinusApi api;

		public MinusHosting (TravelConfig config)
			: base (config)
		{
			api = new MinusApi ("someDummyKey"); // api keys aren't working yet

			api.UploadItemComplete += (sender, result) => Log.Info ("Update complete: ", result.Id, " (", result.Width, "x", result.Height, ")");

			api.UploadItemFailed += (sender, e) => Console.WriteLine ("Failed to get items from gallery...\n" + e.Message);
		}

		public override void Host (PhotoCollection.Photo photo, VirtualFile file)
		{
			Log.Info ("Upload to min.us: ", photo.Filename);
			string url = Upload (image: file.OpenReader ().ReadBytes (), filename: photo.Filename);
			Log.Info ("  => result: ", url);
			photo.HostedURL = url;
		}

		string Upload (byte[] image, string filename)
		{
			return null;
		}
	}
}
