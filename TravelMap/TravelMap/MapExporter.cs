using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Core.IO;
using Core.Portable;
using Core.Shell.Common.FileSystems;
using Core.Shell.Platform.FileSystems;

namespace TravelMap
{
	public class MapExporter
	{
		static readonly string FILE_HTML = "travelmap.html";
		static readonly string FILE_JS = "travelmap.js";
		static readonly string DIRECTORY_ASSETS = "assets";
		static readonly string DIRECTORY_THUMBNAILS = "thumbnails";

		readonly TravelConfig config;

		public MapExporter (TravelConfig config)
		{
			this.config = config;
		}

		public void ExportHTML5 ()
		{
			RegularDirectory outputDirectory = config.Config.Html5OutputDirectory as RegularDirectory;
			RegularDirectory appDirectory = FileSystemSubsystems.ParseNativePath (System.IO.Path.GetDirectoryName (PlatformInfo.System.ApplicationPath)) as RegularDirectory;
			RegularDirectory cacheThumbnailDirectory = config.Config.ThumbnailCacheDirectory as RegularDirectory;

			if (outputDirectory == null) {
				Log.Error ("Error: No Html5OutputDirectory!");
				return;
			}
			if (appDirectory == null) {
				Log.Error ("Error: No valid appDirectory!");
				return;
			}
			if (cacheThumbnailDirectory == null) {
				Log.Error ("Error: No valid cacheThumbnailDirectory!");
				return;
			}

			Log.Info ("Html5 Export:");
			Log.Indent++;

			Log.Info ("output directory: ", outputDirectory);

			outputDirectory.GetChildFile (FILE_HTML).OpenWriter ().WriteLines (appDirectory.GetChildFile (FILE_HTML).OpenReader ().ReadLines ());
			outputDirectory.GetChildFile (FILE_JS).OpenWriter ().WriteLines (template_JS (appDirectory.GetChildFile (FILE_JS).OpenReader ().ReadLines ()));

			RegularDirectory appAssetDirectory = appDirectory.GetChildDirectory (DIRECTORY_ASSETS) as RegularDirectory;
			RegularDirectory outputAssetDirectory = outputDirectory.GetChildDirectory (DIRECTORY_ASSETS) as RegularDirectory;
			outputAssetDirectory.CreateDirectories ();
			foreach (RegularFile assetFile in appAssetDirectory.OpenList().ListFiles()) {
				outputAssetDirectory.GetChildFile (assetFile.Path.FileName).OpenWriter ().WriteBytes (assetFile.OpenReader ().ReadBytes ());
			}

			RegularDirectory outputThumbnailDirectory = outputDirectory.GetChildDirectory (DIRECTORY_THUMBNAILS) as RegularDirectory;
			outputThumbnailDirectory.CreateDirectories ();
			foreach (RegularFile thumbnailFile in cacheThumbnailDirectory.OpenList().ListFiles()) {
				outputThumbnailDirectory.GetChildFile (thumbnailFile.Path.FileName).OpenWriter ().WriteBytes (thumbnailFile.OpenReader ().ReadBytes ());
			}

			Log.Indent--;
		}

		IEnumerable<string> template_JS (IEnumerable<string> enumerable)
		{
			foreach (string line in enumerable) {
				yield return line;

				if (line.Contains ("// HOOK: SET JSON DATA")) {
					yield return "dataArray = ";
					var photos = config.Photos.Photos.Photos;
					yield return PortableConfigHelper.WriteConfig (stuff: photos, inline: true);
					yield return ";";
					//yield return "dataArray = dataArray['photos'];";
				}
			}
		}
	}
}

