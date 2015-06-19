using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Common;
using Core.Shell.Common.FileSystems;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Text;

namespace TravelMap.Hosting
{
	public class ImgurHosting : PhotoHosting
	{
		static readonly string FUCK_YOU = "b014cb8e3959032";

		public ImgurHosting (TravelConfig config)
			: base (config)
		{
		}

		public override void Host (PhotoCollection.Photo photo, VirtualFile file)
		{
			Log.Info ("Upload to imgur: ", photo.Filename);
			string url = Upload (image: file.OpenReader ().ReadBytes (), filename: photo.Filename);
			Log.Info ("  => result: ", url);
			photo.HostedURL = url;
		}

		static string Upload (byte[] image, string filename)
		{
			try {
				using (var w = new WebClient ()) {
					var values = new NameValueCollection {
						{ "image", Convert.ToBase64String (image) },
						{ "name", filename },
						{ "title", filename },
					};

					w.Headers.Add ("Authorization", "Client-ID " + FUCK_YOU);
					string response = Encoding.UTF8.GetString (w.UploadValues ("https://api.imgur.com/3/upload.xml", values));
					Log.Info (response);
					return response;
				}
			} catch (Exception ex) {
				Log.Warning (ex);
				return null;
			}
		}
	}
}
