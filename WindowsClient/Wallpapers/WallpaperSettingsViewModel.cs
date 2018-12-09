﻿using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public class WallpaperSettingsViewModel : INotifyPropertyChanged
	{
		public WallpaperSettingsViewModel(SettingsProvider settingsProvider)
		{
			this.settingsProvider = settingsProvider;

			settings = settingsProvider.LoadWallpaperSettings();
			ConfigurationCollection = new ObservableCollection<WallpaperConfigurationViewModel>();

			foreach (WallpaperConfiguration configuration in settings.ConfigurationCollection)
			{
				WallpaperConfigurationViewModel configurationViewModel = new WallpaperConfigurationViewModel(configuration);
				configurationViewModel.PropertyChanged += HandleConfigurationChanged;
				ConfigurationCollection.Add(configurationViewModel);
			}

			WarningCollection = settings.Validate();

			SaveSettingsCommand = new DelegateCommand<object>(_ => SaveSettings(), _ => CanSaveSettings());
			AddConfigurationCommand = new DelegateCommand<object>(_ => AddConfiguration());
			RemoveConfigurationCommand = new DelegateCommand<WallpaperConfigurationViewModel>(RemoveConfiguration);
		}

		private readonly SettingsProvider settingsProvider;
		private WallpaperSettings settings;

		public event PropertyChangedEventHandler PropertyChanged;

		public Dictionary<string, List<Exception>> WarningCollection { get; private set; }

		public bool HasWarnings { get { return WarningCollection.SelectMany(kvp => kvp.Value).Any(); } }

		private void UpdateValidation()
		{
			WarningCollection = settings.Validate();

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasWarnings)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WarningCollection)));

			SaveSettingsCommand.RaiseCanExecuteChanged();
		}

		public ObservableCollection<WallpaperConfigurationViewModel> ConfigurationCollection { get; }

		private void HandleConfigurationChanged(object sender, PropertyChangedEventArgs eventArguments)
		{
			UpdateValidation();
		}

		public DelegateCommand<object> SaveSettingsCommand { get; }
		public DelegateCommand<object> AddConfigurationCommand { get; }
		public DelegateCommand<WallpaperConfigurationViewModel> RemoveConfigurationCommand { get; }

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

			configurationViewModel.PropertyChanged += HandleConfigurationChanged;
			settings.ConfigurationCollection.Add(configuration);
			ConfigurationCollection.Add(configurationViewModel);

			UpdateValidation();
		}

		private void RemoveConfiguration(WallpaperConfigurationViewModel configuration)
		{
			configuration.PropertyChanged -= HandleConfigurationChanged;
			settings.ConfigurationCollection.Remove(configuration.Model);
			ConfigurationCollection.Remove(configuration);

			UpdateValidation();
		}
	}
}
