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

			ConfigurationCollection = new ObservableCollection<WallpaperConfigurationViewModel>();
			foreach (WallpaperConfiguration configuration in settingsProvider.LoadWallpaperSettings())
				ConfigurationCollection.Add(new WallpaperConfigurationViewModel(configuration));

			AddConfigurationCommand = new DelegateCommand<object>(_ => AddConfiguration());
			RemoveConfigurationCommand = new DelegateCommand<WallpaperConfigurationViewModel>(RemoveConfiguration);
		}

		private readonly SettingsProvider settingsProvider;

		public ObservableCollection<WallpaperConfigurationViewModel> ConfigurationCollection { get; }

		public DelegateCommand<object> AddConfigurationCommand { get; }
		public DelegateCommand<WallpaperConfigurationViewModel> RemoveConfigurationCommand { get; }

		private void AddConfiguration()
		{
			WallpaperConfiguration configuration = new WallpaperConfiguration()
			{
				Name = "New Configuration",
				CyclePeriod = TimeSpan.FromMinutes(5),
			};

			ConfigurationCollection.Add(new WallpaperConfigurationViewModel(configuration));
		}

		private void RemoveConfiguration(WallpaperConfigurationViewModel configuration)
		{
			ConfigurationCollection.Remove(configuration);
		}
	}
}
