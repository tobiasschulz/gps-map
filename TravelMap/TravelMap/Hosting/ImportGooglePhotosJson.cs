using System;
using System.Collections.Generic;
using System.Linq;
using Core.IO;
using Core.Shell.Common.FileSystems;
using Newtonsoft.Json;

namespace TravelMap.Hosting
{
	public class ImportGooglePhotosJson : PhotoHosting
	{
		public ImportGooglePhotosJson (TravelConfig config)
			: base (config)
		{
		}

		public void Import ()
		{
			string json = System.IO.File.ReadAllText ("/ssd/Cloud/Bilder/photos.json");
			JsonAlbumCollection albums = PortableConfigHelper.ReadConfig<JsonAlbumCollection> (content: ref json);

			foreach (var photo in config.Photos.Photos.Photos) {
				JsonPhoto jsonPhoto = albums.Albums.Values.SelectMany<JsonAlbum, JsonPhoto> (p => p.Photos).FirstOrDefault (p => p.Filename == photo.Filename);

				if (jsonPhoto != null) {
					photo.HostedURL = jsonPhoto.HostedURL;
				}
			}
		}

		public class JsonAlbumCollection
		{

			[JsonProperty ("albums")]
			public Dictionary<string, JsonAlbum> Albums { get; set; } = new Dictionary<string, JsonAlbum>();
		}

		public class JsonAlbum
		{
			[JsonProperty ("album_title")]
			public string Title { get; set; } = "";

			[JsonProperty ("photos")]
			public JsonPhoto[] Photos { get; set; } = new JsonPhoto[0];
		}

		public class JsonPhoto
		{
			[JsonProperty ("google_photos_id")]
			public string GoogleId { get; set; } = "";

			[JsonProperty ("filename")]
			public string Filename { get; set; } = "";

			[JsonProperty ("width")]
			public int Width { get; set; } = 0;

			[JsonProperty ("height")]
			public int Height { get; set; } = 0;

			[JsonProperty ("url_hosted")]
			public string HostedURL { get; set; } = null;
		}

		public override void Host (PhotoCollection.Photo photo, VirtualFile file)
		{
		}
	}
}

