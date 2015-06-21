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
using Core.Common;
using Core.Shell.Common.FileSystems;
using Newtonsoft.Json;

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
			if (CheckChredits ()) {
				Log.Info ("Upload to imgur: ", photo.Filename);
				string url = Upload (image: file.OpenReader ().ReadBytes (), filename: photo.Filename);
				Log.Info ("  => result: ", url);
				photo.HostedURL = url;
			}
		}

		static string Upload (byte[] image, string filename)
		{
			try {
				using (var w = new WebClient ()) {
					
					w.Headers.Add ("Authorization", "Client-ID " + FUCK_YOU);

					var values = new NameValueCollection {
						{ "image", Convert.ToBase64String (image) },
						{ "name", filename },
						{ "title", filename },
					};

					string response = Encoding.UTF8.GetString (w.UploadValues ("https://api.imgur.com/3/upload.xml", values));
					Log.Info (response);
					if (response.Contains ("<link>")) {
						response = response.Between (left: "<link>", right: "</link>");
					}
					return response;
				}
			} catch (Exception ex) {
				Log.Warning (ex);
				return null;
			}
		}

		static bool CheckChredits ()
		{
			bool result = false;
			try {
				using (var w = new WebClient ()) {

					w.Headers.Add ("Authorization", "Client-ID " + FUCK_YOU);

					string creditsString = w.DownloadString ("https://api.imgur.com/3/credits");
					CreditResponse creditsObject = JsonConvert.DeserializeObject<CreditResponse> (creditsString);

					if (creditsObject.success) {
						int clientLimit = creditsObject.data.ClientLimit;
						int clientRemaining = creditsObject.data.ClientRemaining;
						if (clientRemaining > clientLimit * 0.01) {
							Log.Info ("Imgur credit check: successful: limit=", clientLimit, ", remaining=", clientRemaining);
							result = true;
						} else {
							Log.Warning ("Imgur credit check: not successful: limit=", clientLimit, ", remaining=", clientRemaining);
						}

					} else {
						Log.Warning ("Imgur credit check: invalid format: ", creditsString);
					}
				}
			} catch (Exception ex) {
				Log.Warning ("Imgur credit check: ", ex);
			}
			return result;
		}

		class CreditResponse
		{
			public bool success = false;
			public int status = -1;
			public CreditResponseData data = new CreditResponseData{ };

			public class CreditResponseData
			{
				public int UserLimit = 0;
				public int UserRemaining = 0;
				public int UserReset = 0;
				public int ClientLimit = 0;
				public int ClientRemaining = 0;
			}
		}
	}
}
