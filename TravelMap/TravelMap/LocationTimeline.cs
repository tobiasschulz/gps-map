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

		public bool Contains (Func<PortableLocation, bool> where)
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

		void Save ()
		{
			ConfigHelper.SaveConfig (fullPath: fullPath, stuff: locations);
		}
	}
}

