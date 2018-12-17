using NLog;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;

namespace Overmind.ImageManager.WallpaperService
{
	public class MainViewModel : INotifyPropertyChanged, IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(MainViewModel));

		public MainViewModel(SettingsProvider settingsProvider, DataProvider dataProvider, IQueryEngine<ImageModel> queryEngine, string wallpaperStorage)
		{
			this.settingsProvider = settingsProvider;
			this.dataProvider = dataProvider;
			this.queryEngine = queryEngine;
			this.wallpaperStorage = wallpaperStorage;

			ApplyConfigurationCommand = new DelegateCommand<object>(_ => ApplyConfiguration());
			ReloadSettingsCommand = new DelegateCommand<object>(_ => ReloadSettings());
			NextWallpaperCommand = new DelegateCommand<object>(_ => wallpaperService.CycleNow(), _ => wallpaperService != null);
			CopyWallpaperHashCommand = new DelegateCommand<object>(_ => Clipboard.SetText(wallpaperService.CurrentWallpaper.Hash), _ => wallpaperService != null);
		}

		private readonly SettingsProvider settingsProvider;
		private readonly DataProvider dataProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly string wallpaperStorage;
		private WallpaperSettings wallpaperSettings;
		private WallpaperServiceInstance wallpaperService;

		public event PropertyChangedEventHandler PropertyChanged;

		public IEnumerable<WallpaperConfiguration> ConfigurationCollection { get { return wallpaperSettings.ConfigurationCollection; } }
		public WallpaperConfiguration ActiveConfiguration { get; set; }

		public DelegateCommand<object> ApplyConfigurationCommand { get; }
		public DelegateCommand<object> ReloadSettingsCommand { get; }
		public DelegateCommand<object> NextWallpaperCommand { get; }
		public DelegateCommand<object> CopyWallpaperHashCommand { get; }

		public void Dispose()
		{
			if (wallpaperService != null)
			{
				wallpaperService.Dispose();
				wallpaperService = null;
			}
		}

		public void ApplyConfiguration()
		{
			if (wallpaperService != null)
			{
				wallpaperService.Dispose();
				wallpaperService = null;
			}

			if (ActiveConfiguration != null)
			{
				try
				{
					wallpaperService = WallpaperServiceInstance.CreateInstance(ActiveConfiguration, dataProvider, queryEngine, SetWallpaper, new Random());
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Failed to create wallpaper service instance");
				}

				try
				{
					settingsProvider.SaveActiveWallpaperConfiguration(ActiveConfiguration.Name);
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Failed to save active wallpaper configuration");
				}
			}

			NextWallpaperCommand.RaiseCanExecuteChanged();
			CopyWallpaperHashCommand.RaiseCanExecuteChanged();
		}

		public void ReloadSettings()
		{
			wallpaperSettings = TryLoadSettings();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfigurationCollection)));

			ActiveConfiguration = TryLoadActiveConfiguration();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveConfiguration)));
		}

		private WallpaperSettings TryLoadSettings()
		{
			WallpaperSettings wallpaperSettings = null;

			try
			{
				wallpaperSettings = settingsProvider.LoadWallpaperSettings();
				if (wallpaperSettings == null)
					throw new ArgumentNullException("Value cannot be null", nameof(wallpaperSettings));
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load wallpaper settings");
			}

			if (wallpaperSettings == null)
				return new WallpaperSettings();

			{
				Dictionary<string, List<Exception>> settingsValidation = wallpaperSettings.Validate();
				foreach (Exception validationException in settingsValidation.SelectMany(kvp => kvp.Value))
					Logger.Warn("Failed to validate wallpaper settings: {0}", FormatExtensions.FormatExceptionHint(validationException));
				if (settingsValidation.SelectMany(kvp => kvp.Value).Any())
					return new WallpaperSettings();
			}

			foreach (WallpaperConfiguration configuration in wallpaperSettings.ConfigurationCollection.ToList())
			{
				Dictionary<string, List<Exception>> configurationValidation = configuration.Validate(queryEngine);
				foreach (Exception validationException in configurationValidation.SelectMany(kvp => kvp.Value))
					Logger.Warn("Failed to validate wallpaper configuration '{0}': {1}", configuration.Name, FormatExtensions.FormatExceptionHint(validationException));
				if (configurationValidation.SelectMany(kvp => kvp.Value).Any())
					wallpaperSettings.ConfigurationCollection.Remove(configuration);
			}

			return wallpaperSettings;
		}

		private WallpaperConfiguration TryLoadActiveConfiguration()
		{
			try
			{
				string activeConfiguration = settingsProvider.LoadActiveWallpaperConfiguration();
				return wallpaperSettings.ConfigurationCollection.Single(configuration => configuration.Name == activeConfiguration);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load active configuration");
			}

			return null;
		}

		private void SetWallpaper(string imagePath)
		{
			string savePath = Path.Combine(wallpaperStorage, "Wallpaper.jpg");
			WindowsWallpaper.Save(imagePath, savePath, ImageFormat.Jpeg, 100);
			WindowsWallpaper.Set(savePath);
		}
	}
}
