using System;
using Core.Common;
using Core.Shell.Common.FileSystems;
using FlickrNet;

namespace TravelMap.Hosting
{
	public class FlickrHosting : PhotoHosting
	{
		static readonly string DORYCERA_PICTIPENNIS = "89cbd41c6bcab82bb4ff34963c128539";
		static readonly string NEWFOUNDLAND_AND_LABRADOR = "6f796d3bfccfcce1";

		readonly Flickr flickr;

		public FlickrHosting (TravelConfig config)
			: base (config)
		{
			flickr = new Flickr (DORYCERA_PICTIPENNIS, NEWFOUNDLAND_AND_LABRADOR);

			var requestToken = flickr.OAuthGetRequestToken ("oob");
			string url = flickr.OAuthCalculateAuthorizationUrl (requestToken.Token, AuthLevel.Read);
			Log.Info (url);
		}

		public override void Host (PhotoCollection.Photo photo, VirtualFile file)
		{
		}

		public void test ()
		{
			var photos = flickr.PhotosGetRecent (
				             perPage: 1000, 
				             page: 1, 
				             extras: PhotoSearchExtras.LargeUrl | PhotoSearchExtras.Tags
			             );

			foreach (Photo photo in photos) {
				Console.WriteLine ("Photo {0} has title {1}", photo.PhotoId, photo.Title);
			}
		}
	}
}

