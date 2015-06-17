using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Core.Shell.Common.FileSystems;
using Core.Shell.Platform.FileSystems;

namespace TravelMap
{
	public class MapExporter
	{
		static readonly string FILE_HTML = "travelmap.html";
		static readonly string FILE_JS = "travelmap.js";

		readonly TravelConfig config;

		public MapExporter (TravelConfig config)
		{
			this.config = config;
		}

		public void ExportHTML5 ()
		{
			RegularDirectory outputDirectory = config.Config.Html5OutputDirectory as RegularDirectory;

			if (outputDirectory == null) {
				Log.Error ("Error: No Html5OutputDirectory!");
				return;
			}

			Log.Info ("Html5 Export:");
			Log.Indent++;

			Log.Info ("output directory: ", outputDirectory);
			outputDirectory.GetChildFile (FILE_HTML);

			Log.Indent--;
		}
	}
}

