using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.ObjectModel;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public class WallpaperSettingsViewModel
	{
		public WallpaperSettingsViewModel(SettingsProvider settingsProvider)
		{
			this.settingsProvider = settingsProvider;

			settings = settingsProvider.LoadWallpaperSettings();
			ConfigurationCollection = new ObservableCollection<WallpaperConfigurationViewModel>();
			foreach (WallpaperConfiguration configuration in settings.Configurations)
				ConfigurationCollection.Add(new WallpaperConfigurationViewModel(configuration));

			SaveSettingsCommand = new DelegateCommand<object>(_ => SaveSettings());
			AddConfigurationCommand = new DelegateCommand<object>(_ => AddConfiguration());
			RemoveConfigurationCommand = new DelegateCommand<WallpaperConfigurationViewModel>(RemoveConfiguration);
		}

		private readonly SettingsProvider settingsProvider;
		private WallpaperSettings settings;

		public ObservableCollection<WallpaperConfigurationViewModel> ConfigurationCollection { get; }

		public DelegateCommand<object> SaveSettingsCommand { get; }
		public DelegateCommand<object> AddConfigurationCommand { get; }
		public DelegateCommand<WallpaperConfigurationViewModel> RemoveConfigurationCommand { get; }

		private void SaveSettings()
		{
			settingsProvider.SaveWallpaperSettings(settings);
		}

		private void AddConfiguration()
		{
			WallpaperConfigurationViewModel configuration = new WallpaperConfigurationViewModel(
				new WallpaperConfiguration() { Name = "New Configuration", CyclePeriod = TimeSpan.FromMinutes(5) });

			settings.Configurations.Add(configuration.Model);
			ConfigurationCollection.Add(configuration);
		}

		private void RemoveConfiguration(WallpaperConfigurationViewModel configuration)
		{
			settings.Configurations.Remove(configuration.Model);
			ConfigurationCollection.Remove(configuration);
		}
	}
}
