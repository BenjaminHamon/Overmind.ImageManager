using Newtonsoft.Json;
using NLog;
using Overmind.ImageManager.Model.Wallpapers;
using System.IO;

namespace Overmind.ImageManager.Model
{
	public class SettingsProvider
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(SettingsProvider));

		private const string WallpaperSettingsFile = "WallpaperService.json";
		private const string ActiveWallpaperConfigurationFile = "WallpaperService.active.txt";

		public SettingsProvider(JsonSerializer serializer, string settingsDirectory)
		{
			this.serializer = serializer;
			this.settingsDirectory = settingsDirectory;
		}

		private readonly JsonSerializer serializer;
		private readonly string settingsDirectory;

		public WallpaperSettings LoadWallpaperSettings()
		{
			Logger.Info("Loading wallpaper settings");

			string path = Path.Combine(settingsDirectory, WallpaperSettingsFile);

			if (File.Exists(path) == false)
				return new WallpaperSettings();

			using (StreamReader streamReader = new StreamReader(path))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize<WallpaperSettings>(jsonReader);
		}

		public void SaveWallpaperSettings(WallpaperSettings settings)
		{
			Logger.Info("Saving wallpaper settings");

			string path = Path.Combine(settingsDirectory, WallpaperSettingsFile);

			using (StreamWriter streamWriter = new StreamWriter(path))
			using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
				serializer.Serialize(jsonWriter, settings);
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
