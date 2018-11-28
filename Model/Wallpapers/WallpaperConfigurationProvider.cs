using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Overmind.ImageManager.Model.Wallpapers
{
	public class WallpaperConfigurationProvider
	{
		private const string ConfigurationFile = "Wallpapers.json";
		private const string ActiveConfigurationFile = "Wallpaper.active.txt";

		public WallpaperConfigurationProvider(JsonSerializer serializer, string configurationDirectory)
		{
			this.serializer = serializer;
			this.configurationDirectory = configurationDirectory;
		}

		private readonly JsonSerializer serializer;
		private readonly string configurationDirectory;

		public string GetConfigurationPath()
		{
			return Path.Combine(configurationDirectory, ConfigurationFile);
		}

		public ICollection<WallpaperConfiguration> LoadConfiguration()
		{
			string path = Path.Combine(configurationDirectory, ConfigurationFile);

			if (File.Exists(path) == false)
				return new List<WallpaperConfiguration>();

			using (StreamReader streamReader = new StreamReader(path))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize<List<WallpaperConfiguration>>(jsonReader);
		}

		public string LoadActiveConfiguration()
		{
			string path = Path.Combine(configurationDirectory, ActiveConfigurationFile);
			if (File.Exists(path) == false)
				return null;
			return File.ReadAllText(path).Trim();
		}

		public void SaveActiveConfiguration(string configurationName)
		{
			string path = Path.Combine(configurationDirectory, ActiveConfigurationFile);
			File.WriteAllText(path, configurationName);
		}

		public string GetWallpaperSavePath()
		{
			return Path.Combine(configurationDirectory, "Wallpaper");
		}
	}
}
