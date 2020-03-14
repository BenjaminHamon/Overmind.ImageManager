using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.ImageManager.WindowsClient.Wallpapers;

namespace Overmind.ImageManager.WindowsClient
{
	public class SettingsViewModel
	{
		public SettingsViewModel(SettingsProvider settingsProvider, IQueryEngine<ImageModel> queryEngine)
		{
			DownloaderSettings = new DownloaderSettingsViewModel(settingsProvider);
			WallpaperSettings = new WallpaperSettingsViewModel(settingsProvider, queryEngine);
		}

		public DownloaderSettingsViewModel DownloaderSettings { get; }
		public WallpaperSettingsViewModel WallpaperSettings { get; }
	}
}
