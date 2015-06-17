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

			LocationCollector locations = new LocationCollector (config: config);
			locations.SyncFromFiles ();

			DesktopPlatform.Finish ();
		}
	}
}
