using NLog;
using Overmind.ImageManager.Model.Serialization;
using Overmind.ImageManager.Model.Wallpapers;
using System.IO;

namespace Overmind.ImageManager.Model
{
	public class SettingsProvider
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(SettingsProvider));

		private const string WallpaperSettingsFile = "WallpaperService.json";
		private const string ActiveWallpaperConfigurationFile = "WallpaperService.active.txt";

		public SettingsProvider(ISerializer serializer, string settingsDirectory)
		{
			this.serializer = serializer;
			this.settingsDirectory = settingsDirectory;
		}

		private readonly ISerializer serializer;
		private readonly string settingsDirectory;

		public WallpaperSettings LoadWallpaperSettings()
		{
			Logger.Info("Loading wallpaper settings");

			string path = Path.Combine(settingsDirectory, WallpaperSettingsFile);

			if (File.Exists(path) == false)
				return new WallpaperSettings();

			return serializer.DeserializeFromFile<WallpaperSettings>(path);
		}

		public void SaveWallpaperSettings(WallpaperSettings settings)
		{
			Logger.Info("Saving wallpaper settings");

			string path = Path.Combine(settingsDirectory, WallpaperSettingsFile);
			serializer.SerializeToFile(path, settings);
		}

		public string LoadActiveWallpaperConfiguration()
		{
			string path = Path.Combine(settingsDirectory, ActiveWallpaperConfigurationFile);
			if (File.Exists(path) == false)
				return null;
			return File.ReadAllText(path).Trim();
		}

		public void SaveActiveWallpaperConfiguration(string configurationName)
		{
			string path = Path.Combine(settingsDirectory, ActiveWallpaperConfigurationFile);
			File.WriteAllText(path, configurationName);
		}
	}
}
