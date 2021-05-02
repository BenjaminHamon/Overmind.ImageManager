using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Downloads;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class DownloaderSettingsViewModel : INotifyPropertyChanged
	{
		public DownloaderSettingsViewModel(SettingsProvider settingsProvider)
		{
			this.settingsProvider = settingsProvider;

			settings = new DownloaderSettings();
			SourceConfigurationCollection = new ObservableCollection<DownloadSourceConfigurationViewModel>();

			WarningCollection = new Dictionary<string, List<Exception>>();

			SaveSettingsCommand = new DelegateCommand<object>(_ => SaveSettings(), _ => CanSaveSettings());
			AddConfigurationCommand = new DelegateCommand<object>(_ => AddConfiguration());
			RemoveConfigurationCommand = new DelegateCommand<DownloadSourceConfigurationViewModel>(RemoveConfiguration);
		}

		private readonly SettingsProvider settingsProvider;
		private DownloaderSettings settings;

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

		public ObservableCollection<DownloadSourceConfigurationViewModel> SourceConfigurationCollection { get; private set; }

		private void HandleConfigurationChanged(object sender, PropertyChangedEventArgs eventArguments)
		{
			UpdateValidation();
		}

		public DelegateCommand<object> SaveSettingsCommand { get; }
		public DelegateCommand<object> AddConfigurationCommand { get; }
		public DelegateCommand<DownloadSourceConfigurationViewModel> RemoveConfigurationCommand { get; }

		public void ReloadSettings()
		{
			settings = settingsProvider.LoadApplicationSettings().DownloaderSettings
				?? throw new ArgumentNullException(nameof(settings), "Wallpaper settings must not be null");

			foreach (DownloadSourceConfigurationViewModel sourceConfigurationViewModel in SourceConfigurationCollection)
			{
				sourceConfigurationViewModel.PropertyChanged -= HandleConfigurationChanged;
			}

			SourceConfigurationCollection = new ObservableCollection<DownloadSourceConfigurationViewModel>();

			foreach (DownloadSourceConfiguration sourceConfiguration in settings.SourceConfigurationCollection)
			{
				DownloadSourceConfigurationViewModel sourceConfigurationViewModel = new DownloadSourceConfigurationViewModel(sourceConfiguration);
				sourceConfigurationViewModel.PropertyChanged += HandleConfigurationChanged;
				SourceConfigurationCollection.Add(sourceConfigurationViewModel);
			}

			UpdateValidation();

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		}

		private bool CanSaveSettings()
		{
			return SourceConfigurationCollection.All(configuration => configuration.HasErrors == false);
		}

		private void SaveSettings()
		{
			settingsProvider.UpdateApplicationSettings(applicationSettings => { applicationSettings.DownloaderSettings = settings; });
		}

		private void AddConfiguration()
		{
			DownloadSourceConfiguration configuration = new DownloadSourceConfiguration() { Name = "New Configuration" };
			DownloadSourceConfigurationViewModel configurationViewModel = new DownloadSourceConfigurationViewModel(configuration);

			configurationViewModel.PropertyChanged += HandleConfigurationChanged;
			settings.SourceConfigurationCollection.Add(configuration);
			SourceConfigurationCollection.Add(configurationViewModel);

			UpdateValidation();
		}

		private void RemoveConfiguration(DownloadSourceConfigurationViewModel configuration)
		{
			configuration.PropertyChanged -= HandleConfigurationChanged;
			settings.SourceConfigurationCollection.Remove(configuration.Model);
			SourceConfigurationCollection.Remove(configuration);

			UpdateValidation();
		}
	}
}
