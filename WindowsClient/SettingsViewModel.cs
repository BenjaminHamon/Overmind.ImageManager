using Overmind.ImageManager.Model;
using Overmind.ImageManager.WindowsClient.Wallpapers;

namespace Overmind.ImageManager.WindowsClient
{
	public class SettingsViewModel
	{
		public SettingsViewModel(SettingsProvider settingsProvider)
		{
			WallpaperSettings = new WallpaperSettingsViewModel(settingsProvider);
		}

		public WallpaperSettingsViewModel WallpaperSettings { get; }
	}
}
