using System;
using Core.Common;
using Core.Net;
using Core.Platform;
using Core.Shell.Platform.FileSystems;
using TravelMap.Hosting;

namespace TravelMap
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			DesktopPlatform.Start ();
			Networking.DisableCertificateValidation ();
			RegularFileSystems.Register ();
			DesktopPlatform.LogTargets.StandardOutput_MinimalType = Log.Type.INFO;

			TravelConfig config = new TravelConfig (configDirectory: null);

			Log.Info ("Hello World!");

			LocationIndex locationIndex = new LocationIndex (config: config);
			locationIndex.SyncFromFiles ();

			ImportGooglePhotosJson importGoogle = new ImportGooglePhotosJson (config: config);
			importGoogle.Import ();

			ImgurHosting hosting = new ImgurHosting (config: config);

			MapExporter mapExport = new MapExporter (config: config);
			mapExport.ExportHTML5 ();

			PhotoIndex photoIndex = new PhotoIndex (config: config, hoster: hosting);
			photoIndex.SyncFromFiles ();

			mapExport.ExportHTML5 ();

			DesktopPlatform.Finish ();
		}
	}
}
