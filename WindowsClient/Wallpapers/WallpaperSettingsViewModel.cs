using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
			{
				WallpaperConfigurationViewModel configurationViewModel = new WallpaperConfigurationViewModel(configuration);
				configurationViewModel.PropertyChanged += HandleValidationUpdated;
				ConfigurationCollection.Add(configurationViewModel);
			}

			SaveSettingsCommand = new DelegateCommand<object>(_ => SaveSettings(), _ => CanSaveSettings());
			AddConfigurationCommand = new DelegateCommand<object>(_ => AddConfiguration());
			RemoveConfigurationCommand = new DelegateCommand<WallpaperConfigurationViewModel>(RemoveConfiguration);
		}

		private readonly SettingsProvider settingsProvider;
		private WallpaperSettings settings;

		public ObservableCollection<WallpaperConfigurationViewModel> ConfigurationCollection { get; }

		public DelegateCommand<object> SaveSettingsCommand { get; }
		public DelegateCommand<object> AddConfigurationCommand { get; }
		public DelegateCommand<WallpaperConfigurationViewModel> RemoveConfigurationCommand { get; }

		private void HandleValidationUpdated(object sender, PropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.PropertyName == nameof(WallpaperConfigurationViewModel.HasErrors))
				SaveSettingsCommand.RaiseCanExecuteChanged();
		}

		private bool CanSaveSettings()
		{
			return ConfigurationCollection.All(configuration => configuration.HasErrors == false);
		}

		private void SaveSettings()
		{
			settingsProvider.SaveWallpaperSettings(settings);
		}

		private void AddConfiguration()
		{
			WallpaperConfiguration configuration = new WallpaperConfiguration() { Name = "New Configuration", CyclePeriod = TimeSpan.FromMinutes(5) };
			WallpaperConfigurationViewModel configurationViewModel = new WallpaperConfigurationViewModel(configuration);

			configurationViewModel.PropertyChanged += HandleValidationUpdated;
			settings.Configurations.Add(configuration);
			ConfigurationCollection.Add(configurationViewModel);
			SaveSettingsCommand.RaiseCanExecuteChanged();
		}

		private void RemoveConfiguration(WallpaperConfigurationViewModel configuration)
		{
			configuration.PropertyChanged -= HandleValidationUpdated;
			settings.Configurations.Remove(configuration.Model);
			ConfigurationCollection.Remove(configuration);
			SaveSettingsCommand.RaiseCanExecuteChanged();
		}
	}
}
