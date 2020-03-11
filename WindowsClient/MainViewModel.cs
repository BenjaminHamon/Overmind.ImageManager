using NLog;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Downloads;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient
{
	public class MainViewModel : INotifyPropertyChanged, IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(MainViewModel));

		public MainViewModel(WindowsApplication application, SettingsProvider settingsProvider,
			ICollectionProvider collectionProvider, IQueryEngine<ImageModel> queryEngine, IDownloader downloader, Func<Random> randomFactory)
		{
			this.application = application;
			this.settingsProvider = settingsProvider;
			this.collectionProvider = collectionProvider;
			this.queryEngine = queryEngine;
			this.downloader = downloader;
			this.randomFactory = randomFactory;

			ShowDownloaderCommand = new DelegateCommand<object>(_ => application.ShowDownloader());
			ShowSettingsCommand = new DelegateCommand<object>(_ => application.ShowSettings());
			ShowAboutCommand = new DelegateCommand<object>(_ => application.ShowAbout());
			ExitApplicationCommand = new DelegateCommand<object>(_ => application.Shutdown());

			CreateCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(CreateCollection(path), path));
			LoadCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(LoadCollection(path), path));
			SaveCollectionCommand = new DelegateCommand<object>(_ => ActiveCollection.Save(), _ => ActiveCollection != null);
			ExportCollectionCommand = new DelegateCommand<string>(path => ActiveCollection.Export(path), _ => ActiveCollection != null);
			CloseCollectionCommand = new DelegateCommand<object>(_ => CloseCollection(), _ => ActiveCollection != null);

			SpawnSlideShowCommand = new DelegateCommand<object>(_ => application.SpawnSlideShow(ActiveCollection.FilteredImages, false), _ => ActiveCollection != null);
			SpawnShuffledSlideShowCommand = new DelegateCommand<object>(_ => application.SpawnSlideShow(ActiveCollection.FilteredImages, true), _ => ActiveCollection != null);

			ViewImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.ViewImage(image); });
			OpenImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.OpenImageExternally(image, "open"); });
			EditImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.OpenImageExternally(image, "edit"); });
			RestartDownloadCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) Downloader.RestartDownload(image); });
		}

		private readonly WindowsApplication application;
		private readonly SettingsProvider settingsProvider;
		private readonly ICollectionProvider collectionProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly IDownloader downloader;
		private readonly Func<Random> randomFactory;

		public string WindowTitle
		{
			get
			{
				if (ActiveCollection == null)
					return WindowsApplication.ApplicationTitle;
				return ActiveCollection.Name + " - " + WindowsApplication.ApplicationTitle;
			}
		}

		private CollectionViewModel activeCollectionField;
		public CollectionViewModel ActiveCollection
		{
			get { return activeCollectionField; }
			private set
			{
				if (activeCollectionField == value)
					return;
				activeCollectionField = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveCollection)));

				SaveCollectionCommand.RaiseCanExecuteChanged();
				ExportCollectionCommand.RaiseCanExecuteChanged();
				CloseCollectionCommand.RaiseCanExecuteChanged();
				SpawnSlideShowCommand.RaiseCanExecuteChanged();
				SpawnShuffledSlideShowCommand.RaiseCanExecuteChanged();
			}
		}

		private DownloaderViewModel downloaderField;
		public DownloaderViewModel Downloader
		{
			get { return downloaderField; }
			private set
			{
				if (downloaderField == value)
					return;
				downloaderField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Downloader)));
			}
		}

		public void Dispose()
		{
			if (ActiveCollection != null)
				CloseCollection();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public DelegateCommand<object> ShowDownloaderCommand { get; }
		public DelegateCommand<object> ShowSettingsCommand { get; }
		public DelegateCommand<object> ShowAboutCommand { get; }
		public DelegateCommand<object> ExitApplicationCommand { get; }

		public DelegateCommand<string> CreateCollectionCommand { get; }
		public DelegateCommand<string> LoadCollectionCommand { get; }
		public DelegateCommand<object> SaveCollectionCommand { get; }
		public DelegateCommand<string> ExportCollectionCommand { get; }
		public DelegateCommand<object> CloseCollectionCommand { get; }

		public DelegateCommand<object> SpawnSlideShowCommand { get; }
		public DelegateCommand<object> SpawnShuffledSlideShowCommand { get; }

		public DelegateCommand<ImageViewModel> ViewImageCommand { get; }
		public DelegateCommand<ImageViewModel> OpenImageCommand { get; }
		public DelegateCommand<ImageViewModel> EditImageCommand { get; }
		public DelegateCommand<ImageViewModel> RestartDownloadCommand { get; }

		private CollectionData CreateCollection(string collectionPath)
		{
			return collectionProvider.CreateCollection(collectionPath);
		}

		private CollectionData LoadCollection(string collectionPath)
		{
			CollectionData collectionData = collectionProvider.LoadCollection(collectionPath);

			try
			{
				collectionProvider.ClearUnsavedFiles(collectionPath);
			}
			catch (Exception exception)
			{
				Logger.Warn(exception, "Failed to clear unsaved files (Path: '{0}')", collectionPath);
			}

			return collectionData;
		}

		private void ChangeCollection(CollectionData collectionData, string collectionPath)
		{
			if (ActiveCollection != null)
				CloseCollection();

			CollectionModel collectionModel = new CollectionModel(collectionProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(application, collectionModel, queryEngine, randomFactory);
			Downloader = new DownloaderViewModel(downloader, ActiveCollection, settingsProvider, application.Dispatcher);
		}

		private void CloseCollection()
		{
			Downloader.Dispose();
			Downloader = null;

			try
			{
				collectionProvider.ClearUnsavedFiles(ActiveCollection.StoragePath);
			}
			catch (Exception exception)
			{
				Logger.Warn(exception, "Failed to clear unsaved files (Path: '{0}')", ActiveCollection.StoragePath);
			}

			ActiveCollection = null;
		}
	}
}
