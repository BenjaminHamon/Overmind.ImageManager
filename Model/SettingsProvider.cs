using NLog;
using Overmind.ImageManager.Model.Serialization;
using System;
using System.IO;

namespace Overmind.ImageManager.Model
{
	public delegate void SettingsUpdatedHandler(SettingsProvider sender);

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
		private FileSystemWatcher fileSystemWatcher;

		public event SettingsUpdatedHandler ApplicationSettingsUpdated;

		public void InitializeWatchers()
		{
			fileSystemWatcher = new FileSystemWatcher(settingsDirectory, "*.json");

			fileSystemWatcher.Created += RaiseSettingsChanged;
			fileSystemWatcher.Renamed += RaiseSettingsChanged;
			fileSystemWatcher.Changed += RaiseSettingsChanged;
			fileSystemWatcher.Error += HandleWatcherError;

			fileSystemWatcher.EnableRaisingEvents = true;
		}

		public void DisposeWatchers()
		{
			fileSystemWatcher.EnableRaisingEvents = false;

			fileSystemWatcher.Created -= RaiseSettingsChanged;
			fileSystemWatcher.Renamed -= RaiseSettingsChanged;
			fileSystemWatcher.Changed -= RaiseSettingsChanged;
			fileSystemWatcher.Error -= HandleWatcherError;

			fileSystemWatcher.Dispose();
			fileSystemWatcher = null;
		}

		private void RaiseSettingsChanged(object sender, FileSystemEventArgs eventArguments)
		{
			if (eventArguments.Name == ApplicationSettingsFileName)
				ApplicationSettingsUpdated?.Invoke(this);
		}

		private void HandleWatcherError(object sender, ErrorEventArgs eventArguments)
		{
			Logger.Warn(eventArguments.GetException(), "File watcher error (Path: {0})", ((FileSystemWatcher)sender).Path);
		}

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
