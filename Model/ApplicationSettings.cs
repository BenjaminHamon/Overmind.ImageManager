using Overmind.ImageManager.Model.Wallpapers;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class ApplicationSettings
	{
		[DataMember]
		public WallpaperSettings WallpaperSettings { get; set; } = new WallpaperSettings();
	}
}
