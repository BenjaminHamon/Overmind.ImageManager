using Newtonsoft.Json;
using Overmind.ImageManager.Model.Wallpapers;
using System.Collections.Generic;
using System.IO;

namespace Overmind.ImageManager.Model
{
	public class SettingsProvider
	{
		private const string WallpaperSettingsFile = "WallpaperService.json";
		private const string ActiveWallpaperConfigurationFile = "WallpaperService.active.txt";

		public SettingsProvider(JsonSerializer serializer, string settingsDirectory)
		{
			this.serializer = serializer;
			this.settingsDirectory = settingsDirectory;
		}

		private readonly JsonSerializer serializer;
		private readonly string settingsDirectory;

		public string GetWallpaperSettingsFilePath()
		{
			return Path.Combine(settingsDirectory, WallpaperSettingsFile);
		}

		public ICollection<WallpaperConfiguration> LoadWallpaperSettings()
		{
			string path = Path.Combine(settingsDirectory, WallpaperSettingsFile);

			if (File.Exists(path) == false)
				return new List<WallpaperConfiguration>();

			using (StreamReader streamReader = new StreamReader(path))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize<List<WallpaperConfiguration>>(jsonReader);
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
