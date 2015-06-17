using System;
using Core.Common;
using Core.Net;
using Core.Platform;
using Core.Shell.Platform.FileSystems;

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

			LocationCollector locationCollector = new LocationCollector (config: config);
			locationCollector.SyncFromFiles ();

			MapExporter mapExport = new MapExporter (config: config);
			mapExport.ExportHTML5 ();

			DesktopPlatform.Finish ();
		}
	}
}
