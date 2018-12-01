using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Overmind.ImageManager.WallpaperService
{
	public class WallpaperServiceViewModel : INotifyPropertyChanged, IDisposable
	{
		public WallpaperServiceViewModel(SettingsProvider settingsProvider, DataProvider dataProvider, string wallpaperStorage)
		{
			this.settingsProvider = settingsProvider;
			this.dataProvider = dataProvider;
			this.wallpaperStorage = wallpaperStorage;

			ApplyConfigurationCommand = new DelegateCommand<object>(_ => ApplyConfiguration());
			EditConfigurationCommand = new DelegateCommand<object>(_ => EditConfiguration());
			ReloadConfigurationCommand = new DelegateCommand<object>(_ => ReloadConfiguration());
			NextWallpaperCommand = new DelegateCommand<object>(_ => wallpaperService.CycleNow(), _ => wallpaperService != null);
			CopyWallpaperHashCommand = new DelegateCommand<object>(_ => Clipboard.SetText(wallpaperService.CurrentWallpaper.Hash), _ => wallpaperService != null);

			ReloadConfiguration();
			ApplyConfiguration();
		}

		private readonly SettingsProvider settingsProvider;
		private readonly DataProvider dataProvider;
		private readonly string wallpaperStorage;
		private WallpaperServiceInstance wallpaperService;

		public event PropertyChangedEventHandler PropertyChanged;

		public string ApplicationTitle { get { return "Overmind Wallpaper Service"; } }
		public IEnumerable<WallpaperConfiguration> ConfigurationCollection { get; private set; }

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
		public DelegateCommand<object> ReloadConfigurationCommand { get; }
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
					Trace.TraceError("[WallpaperService] Failed to create wallpaper service instance: {0}", exception);
				}

				settingsProvider.SaveActiveWallpaperConfiguration(ActiveConfiguration.Name);
			}

			NextWallpaperCommand.RaiseCanExecuteChanged();
			CopyWallpaperHashCommand.RaiseCanExecuteChanged();
		}

		private void EditConfiguration()
		{
			string settingsPath = settingsProvider.GetWallpaperSettingsFilePath();
			if (File.Exists(settingsPath) == false)
			{
				string installationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				string defaultConfigurationPath = Path.Combine(installationDirectory, "Resources", "WallpaperService.default.json");
				File.Copy(defaultConfigurationPath, settingsPath);
			}

			Process.Start(settingsPath);
		}

		private void ReloadConfiguration()
		{
			ConfigurationCollection = settingsProvider.LoadWallpaperSettings();
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
