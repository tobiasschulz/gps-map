using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
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
		public DateTime End { get; set; }

		[JsonProperty ("offset")]
		public int offset { get; set; }

		public TimeSpan ToTimeSpan ()
		{
			Log.Info ("offset = ", offset, ", timespan = ", TimeSpan.FromHours (offset));
			return TimeSpan.FromHours (offset);
		}

		public static UtcOffset DefaultOffset (DateTime reference)
		{
			return new UtcOffset {
				Start = reference.AddYears (-1),
				End = reference.AddYears (+1),
				offset = (int)TimeZoneInfo.Local.GetUtcOffset (reference).TotalHours
			};
		}

		public static UtcOffset FindOffset (List<UtcOffset> list, DateTime dateTimeLocal)
		{
			foreach (UtcOffset offset in list) {
				if (offset.Start <= dateTimeLocal && offset.End >= dateTimeLocal) {
					return offset;
				}
			}
			return DefaultOffset (reference: dateTimeLocal);
		}
	}
}

