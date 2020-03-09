using NLog;
using Overmind.ImageManager.Model.Serialization;
using System;
using System.IO;

namespace Overmind.ImageManager.Model
{
	public class SettingsProvider
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(SettingsProvider));

		private const string ApplicationSettingsFileName = "ApplicationSettings.json";

		public SettingsProvider(ISerializer serializer, string settingsDirectory)
		{
			this.serializer = serializer;
			this.settingsDirectory = settingsDirectory;
		}

		private readonly ISerializer serializer;
		private readonly string settingsDirectory;

		public ApplicationSettings LoadApplicationSettings()
		{
			Logger.Info("Loading application settings");

			string path = Path.Combine(settingsDirectory, ApplicationSettingsFileName);

			if (File.Exists(path) == false)
				return new ApplicationSettings();

			return serializer.DeserializeFromFile<ApplicationSettings>(path);
		}

		public void SaveApplicationSettings(ApplicationSettings settings)
		{
			Logger.Info("Saving application settings");

			string path = Path.Combine(settingsDirectory, ApplicationSettingsFileName);
			serializer.SerializeToFile(path, settings);
		}

		public void UpdateApplicationSettings(Action<ApplicationSettings> updater)
		{
			ApplicationSettings settings = LoadApplicationSettings() ?? new ApplicationSettings();

			updater(settings);

			SaveApplicationSettings(settings);
		}
	}
}
