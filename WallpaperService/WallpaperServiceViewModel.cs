using NLog;
using Overmind.ImageManager.Model;
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
	public class WallpaperServiceViewModel : INotifyPropertyChanged, IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(WallpaperServiceViewModel));

		public WallpaperServiceViewModel(SettingsProvider settingsProvider, DataProvider dataProvider, string wallpaperStorage)
		{
			this.settingsProvider = settingsProvider;
			this.dataProvider = dataProvider;
			this.wallpaperStorage = wallpaperStorage;

			ApplyConfigurationCommand = new DelegateCommand<object>(_ => ApplyConfiguration());
			ReloadSettingsCommand = new DelegateCommand<object>(_ => ReloadSettings());
			NextWallpaperCommand = new DelegateCommand<object>(_ => wallpaperService.CycleNow(), _ => wallpaperService != null);
			CopyWallpaperHashCommand = new DelegateCommand<object>(_ => Clipboard.SetText(wallpaperService.CurrentWallpaper.Hash), _ => wallpaperService != null);

			ReloadSettings();
			ApplyConfiguration();
		}

		private readonly SettingsProvider settingsProvider;
		private readonly DataProvider dataProvider;
		private readonly string wallpaperStorage;
		private WallpaperSettings wallpaperSettings;
		private WallpaperServiceInstance wallpaperService;

		public event PropertyChangedEventHandler PropertyChanged;

		public string ApplicationTitle { get { return WindowsApplication.ApplicationTitle; } }
		public IEnumerable<WallpaperConfiguration> ConfigurationCollection { get { return wallpaperSettings.ConfigurationCollection; } }

		private WallpaperConfiguration activeConfigurationField;
		public WallpaperConfiguration ActiveConfiguration
		{
			get { return activeConfigurationField; }
			set
			{
				if (activeConfigurationField == value)
					return;
				activeConfigurationField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveConfiguration)));
			}
		}

		public DelegateCommand<object> ApplyConfigurationCommand { get; }
		public DelegateCommand<object> EditConfigurationCommand { get; }
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

		private void ApplyConfiguration()
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
					wallpaperService = WallpaperServiceInstance.CreateInstance(dataProvider, ActiveConfiguration, SetWallpaper, new Random());
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Failed to create wallpaper service instance");
				}

				settingsProvider.SaveActiveWallpaperConfiguration(ActiveConfiguration.Name);
			}

			NextWallpaperCommand.RaiseCanExecuteChanged();
			CopyWallpaperHashCommand.RaiseCanExecuteChanged();
		}

		private void ReloadSettings()
		{
			wallpaperSettings = settingsProvider.LoadWallpaperSettings();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfigurationCollection)));

			string activeConfigurationName = settingsProvider.LoadActiveWallpaperConfiguration();
			ActiveConfiguration = ConfigurationCollection.FirstOrDefault(configuration => configuration.Name == activeConfigurationName);
		}

		private void SetWallpaper(string imagePath)
		{
			string savePath = Path.Combine(wallpaperStorage, "Wallpaper.jpg");
			WindowsWallpaper.Save(imagePath, savePath, ImageFormat.Jpeg, 100);
			WindowsWallpaper.Set(savePath);
		}
	}
}
