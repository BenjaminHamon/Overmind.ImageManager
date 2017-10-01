using System;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model.Wallpapers
{
	[DataContract]
	public class WallpaperConfiguration
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string CollectionPath { get; set; }
		[DataMember]
		public string ImageQuery { get; set; }
		[DataMember]
		public TimeSpan CyclePeriod { get; set; }
	}
}
