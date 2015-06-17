using System;
using Core.IO;
using Core.Math;
using System.Linq;

namespace TravelMap
{
	public class LocationTimeline
	{
		readonly PortableLocationCollection locations;
		readonly string fullPath;

		public PortableLocationCollection Locations { get { return locations; } }

		public LocationTimeline (string fullPath)
		{
			this.fullPath = fullPath;
			this.locations = ConfigHelper.OpenConfig<PortableLocationCollection> (fullPath: fullPath);
		}

		public bool ContainsLocation (Func<PortableLocation, bool> where)
		{
			return locations.Locations.Any (where);
		}

		public void AddLocation (PortableLocation location)
		{
			if (location != null) {
				if (!locations.Locations.Contains (location)) {
					locations.AddLocation (location);
					Save ();
				}
			}
		}

		public PortableLocation InterpolateLocation (DateTime dateTime)
		{
			PortableLocation previousLocation = null;
			foreach (PortableLocation loc in locations.Locations.OrderBy (l => l.DateTime)) {

				if (loc.DateTime >= dateTime) {
					if (previousLocation == null) {
						return loc;
					} else {
						return InterpolateLocation (previous: previousLocation, dateTime: dateTime, next: loc);
					}
				}

				previousLocation = loc;
			}
			return null;
		}

		PortableLocation InterpolateLocation (DateTime dateTime, PortableLocation previous, PortableLocation next)
		{
			TimeSpan before = dateTime - previous.DateTime;
			TimeSpan after = next.DateTime - dateTime;
			double percent = (before.TotalMilliseconds) / (before.TotalMilliseconds + after.TotalMilliseconds);

			PortableLocation result = new PortableLocation {
				DateTime = dateTime,
				Latitude = previous.Latitude + percent * (next.Latitude - previous.Latitude),
				Longitude = previous.Longitude + percent * (next.Longitude - previous.Longitude),
				Altitude = previous.Altitude + percent * (next.Altitude - previous.Altitude),
				Provider = percent >= 0.5 ? next.Provider : previous.Provider,
				ReferenceFile = null,
			};

			double beforeMeters = LocationMath.DistanceMeters (previous, result);
			double afterMeters = LocationMath.DistanceMeters (result, next);
			result.ReferenceNote = string.Format ("{0:P1} from {1} meters ({2} seconds ago) to {3} meters (in {4} seconds) ",
				percent, beforeMeters, before.TotalSeconds, afterMeters, after.TotalSeconds);

			return result;
		}

		void Save ()
		{
			ConfigHelper.SaveConfig (fullPath: fullPath, stuff: locations);
		}
	}
}

