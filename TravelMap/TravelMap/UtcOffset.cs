using System;
using System.Collections.Generic;
using System.Linq;
using Core.IO;
using Core.Math;
using Core.Shell.Common.FileSystems;
using Newtonsoft.Json;

namespace TravelMap
{
	public class UtcOffset
	{
		[JsonProperty ("start")]
		public DateTime Start { get; set; }

		[JsonProperty ("end")]
		public DateTime end { get; set; }

		[JsonProperty ("offset")]
		public int offset { get; set; }
	}

}

