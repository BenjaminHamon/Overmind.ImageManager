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
		public WallpaperServiceViewModel(WallpaperConfigurationProvider configurationProvider, DataProvider dataProvider, string wallpaperStorage)
		{
			this.configurationProvider = configurationProvider;
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

		private readonly WallpaperConfigurationProvider configurationProvider;
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
				NextWallpaperCommand.RaiseCanExecuteChanged();
				CopyWallpaperHashCommand.RaiseCanExecuteChanged();
			}

			WallpaperConfiguration configuration = ActiveConfiguration;
			if (configuration == null)
				return;

			CollectionData collectionData = dataProvider.LoadCollection(configuration.CollectionPath);
			ReadOnlyCollectionModel collectionModel = new ReadOnlyCollectionModel(dataProvider, collectionData, configuration.CollectionPath);
			Action<ImageModel> setWallpaperAction = image => SetWallpaper(collectionModel.GetImagePath(image));
			wallpaperService = WallpaperServiceInstance.CreateInstance(collectionModel, configuration, setWallpaperAction, new Random());
			NextWallpaperCommand.RaiseCanExecuteChanged();
			CopyWallpaperHashCommand.RaiseCanExecuteChanged();

			configurationProvider.SaveActiveConfiguration(configuration.Name);
		}

		private void EditConfiguration()
		{
			string configurationPath = configurationProvider.GetConfigurationPath();
			if (File.Exists(configurationPath) == false)
			{
				string installationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				string defaultConfigurationPath = Path.Combine(installationDirectory, "Resources", "WallpaperService.default.json");
				File.Copy(defaultConfigurationPath, configurationPath);
			}

			Process.Start(configurationPath);
		}

		private void ReloadConfiguration()
		{
			ConfigurationCollection = configurationProvider.LoadConfiguration();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfigurationCollection)));

			string activeConfigurationName = configurationProvider.LoadActiveConfiguration();
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
