﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Math;

namespace TravelMap
{
	public static class LocationMath
	{
		public static double DegreesFromString (string str)
		{
			// 50 deg 2' 48.41" N
			// 8 deg 34' 4.85" E

			double degrees = 0;

			List<string> parts = str.Trim ().Split (new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList ();

			string degreesString = parts.FirstOrDefault (s => s.All (c => char.IsDigit (c) || c == '.' || c == '-'));
			string minutesString = parts.FirstOrDefault (s => s.Count (c => char.IsDigit (c) || c == '.') == s.Length - 1 && s.EndsWith ("'"));
			string secondsString = parts.FirstOrDefault (s => s.Count (c => char.IsDigit (c) || c == '.') == s.Length - 1 && s.EndsWith ("\""));
			string northSouthEastWest = parts.FirstOrDefault (s => s == "N" || s == "S" || s == "E" || s == "W");

			//Core.Common.Log.Info ("degreesString: ", degreesString, ", minutesString: ", minutesString, ", secondsString: ", secondsString, ", str: ", str);

			if (!string.IsNullOrWhiteSpace (degreesString)) {
				degrees += double.Parse (degreesString);
			}
			if (!string.IsNullOrWhiteSpace (minutesString)) {
				degrees += double.Parse (minutesString.Replace ("'", "")) / 60.0;
			}
			if (!string.IsNullOrWhiteSpace (secondsString)) {
				degrees += double.Parse (secondsString.Replace ("\"", "")) / 3600.0;
			}
			if (!string.IsNullOrWhiteSpace (northSouthEastWest)) {
				if (northSouthEastWest == "W" && degrees > 0) {
					degrees = -degrees;
				}
				if (northSouthEastWest == "S" && degrees > 0) {
					degrees = -degrees;
				}
			}

			return degrees;
		}
	}
}

