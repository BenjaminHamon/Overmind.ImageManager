using Overmind.ImageManager.Model.Downloads;
using Overmind.ImageManager.Model.Wallpapers;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class ApplicationSettings
	{
		[DataMember]
		public DownloaderSettings DownloaderSettings { get; set; } = new DownloaderSettings();

		[DataMember]
		public WallpaperSettings WallpaperSettings { get; set; } = new WallpaperSettings();
	}
}
