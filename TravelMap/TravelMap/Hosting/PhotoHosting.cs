using System;
using Core.Shell.Common.FileSystems;

namespace TravelMap.Hosting
{
	public abstract class PhotoHosting
	{
		protected readonly TravelConfig config;

		protected PhotoHosting (TravelConfig config)
		{
			this.config = config;
		}

		public abstract void Host (PhotoCollection.Photo photo, VirtualFile file);
	}
}

