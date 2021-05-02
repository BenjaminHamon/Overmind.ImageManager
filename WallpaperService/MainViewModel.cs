using NLog;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.Model.Wallpapers;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace Overmind.ImageManager.WallpaperService
{
	public class MainViewModel : INotifyPropertyChanged, IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(MainViewModel));

		public MainViewModel(SettingsProvider settingsProvider, ICollectionProvider collectionProvider,
			IQueryEngine<ImageModel> queryEngine, Func<Random> randomFactory, string wallpaperStorage)
		{
			this.settingsProvider = settingsProvider;
			this.collectionProvider = collectionProvider;
			this.queryEngine = queryEngine;
			this.randomFactory = randomFactory;
			this.wallpaperStorage = wallpaperStorage;

			ApplyConfigurationCommand = new DelegateCommand<object>(_ => ApplyConfiguration());
			ReloadSettingsCommand = new DelegateCommand<object>(_ => ReloadSettings());
			NextWallpaperCommand = new DelegateCommand<object>(_ => wallpaperService.CycleNow(), _ => wallpaperService != null);
			CopyWallpaperHashCommand = new DelegateCommand<object>(_ => CopyWallpaperHash(), _ => wallpaperService != null);
		}

		private readonly SettingsProvider settingsProvider;
		private readonly ICollectionProvider collectionProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly Func<Random> randomFactory;
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
					wallpaperService = WallpaperServiceInstance.CreateInstance(
						ActiveConfiguration, collectionProvider, queryEngine, SetWallpaper, randomFactory());
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Failed to create wallpaper service instance");
				}

				try
				{
					settingsProvider.UpdateApplicationSettings(
						applicationSettings =>
						{
							if (applicationSettings.WallpaperSettings == null)
								applicationSettings.WallpaperSettings = new WallpaperSettings();

							applicationSettings.WallpaperSettings.ActiveConfiguration = ActiveConfiguration.Name;
						});
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

			ActiveConfiguration = wallpaperSettings.ConfigurationCollection
				.SingleOrDefault(configuration => configuration.Name == wallpaperSettings.ActiveConfiguration);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveConfiguration)));
		}

		private WallpaperSettings TryLoadSettings()
		{
			WallpaperSettings wallpaperSettings = null;

			try
			{
				wallpaperSettings = settingsProvider.LoadApplicationSettings().WallpaperSettings
					?? throw new ArgumentNullException(nameof(wallpaperSettings), "Wallpaper settings must not be null");
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load wallpaper settings");
			}

			if (wallpaperSettings == null)
			{
				return new WallpaperSettings();
			}

			{
				Dictionary<string, List<Exception>> settingsValidation = wallpaperSettings.Validate();

				foreach (Exception validationException in settingsValidation.SelectMany(kvp => kvp.Value))
				{
					Logger.Warn("Failed to validate wallpaper settings: {0}",
						FormatExtensions.FormatExceptionHint(validationException));
				}

				if (settingsValidation.SelectMany(kvp => kvp.Value).Any())
				{
					return new WallpaperSettings();
				}
			}

			foreach (WallpaperConfiguration configuration in wallpaperSettings.ConfigurationCollection.ToList())
			{
				Dictionary<string, List<Exception>> configurationValidation = configuration.Validate(queryEngine);

				foreach (Exception validationException in configurationValidation.SelectMany(kvp => kvp.Value))
				{
					Logger.Warn("Failed to validate wallpaper configuration '{0}': {1}",
						configuration.Name, FormatExtensions.FormatExceptionHint(validationException));
				}

				if (configurationValidation.SelectMany(kvp => kvp.Value).Any())
				{
					wallpaperSettings.ConfigurationCollection.Remove(configuration);
				}
			}

			return wallpaperSettings;
		}

		private void SetWallpaper(ImageModel image)
		{
			WallpaperConfiguration configuration = ActiveConfiguration;
			WallpaperBuilder builder = new WallpaperBuilder(ImageFormat.Jpeg, 100);
			string sourcePath = collectionProvider.GetImagePath(configuration.CollectionPath, image);
			string savePath = Path.Combine(wallpaperStorage, "Wallpaper.jpg");

			if (ShouldUseSingleScreen(image, sourcePath))
			{
				// Force the image to fit for the primary screen
				Rectangle screenArea = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
				builder.CreateForSingleScreen(sourcePath, savePath, screenArea.Width, screenArea.Height);
			}
			else
			{
				// Let the system handle display
				builder.Create(sourcePath, savePath);
			}

			WindowsWallpaper.Set(savePath);
		}

		private bool ShouldUseSingleScreen(ImageModel image, string imagePath)
		{
			if (image.TagCollection.Contains("SingleScreen"))
				return true;
			if (image.TagCollection.Contains("MultiScreen"))
				return false;

			Rectangle screenArea = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

			using (Image sourceImage = Image.FromFile(imagePath))
			{
				float sourceRatio = (float) sourceImage.Width / sourceImage.Height;
				float screenRatio = (float) screenArea.Width / screenArea.Height;

				// If wider by a value between 0% and 50%, to avoid multi screen wallpaper with a lot of empty space
				return (sourceRatio > screenRatio) && (sourceRatio < screenRatio * 1.8);
			}
		}

		private void CopyWallpaperHash()
		{
			ImageModel wallpaper = wallpaperService.GetCurrentWallpaper();

			if (wallpaper == null)
				return;

			try
			{
				Clipboard.SetText(wallpaper.Hash);
			}
			catch (COMException exception)
			{
				Logger.Error(exception, "Failed to copy wallpaper hash to clipboard");
			}
		}
	}
}
