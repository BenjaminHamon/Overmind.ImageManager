using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient
{
	public class MainViewModel : INotifyPropertyChanged, IDisposable
	{
		public MainViewModel(WindowsApplication application, DataProvider dataProvider, IQueryEngine<ImageModel> queryEngine)
		{
			this.application = application;
			this.dataProvider = dataProvider;
			this.queryEngine = queryEngine;

			ShowDownloaderCommand = new DelegateCommand<object>(_ => application.ShowDownloader());
			ShowSettingsCommand = new DelegateCommand<object>(_ => application.ShowSettings());
			ExitApplicationCommand = new DelegateCommand<object>(_ => application.Shutdown());

			CreateCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(dataProvider.CreateCollection(path), path));
			LoadCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(dataProvider.LoadCollection(path), path));
			SaveCollectionCommand = new DelegateCommand<object>(_ => ActiveCollection.Save(), _ => ActiveCollection != null);
			ExportCollectionCommand = new DelegateCommand<string>(path => ActiveCollection.Export(path), _ => ActiveCollection != null);
			CloseCollectionCommand = new DelegateCommand<object>(_ => CloseCollection(), _ => ActiveCollection != null);

			SpawnSlideShowCommand = new DelegateCommand<object>(_ => application.SpawnSlideShow(ActiveCollection.FilteredImages), _ => ActiveCollection != null);
			ViewImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.ViewImage(image); });
			OpenImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.OpenImageExternally(image, "open"); });
			EditImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.OpenImageExternally(image, "edit"); });
			RestartDownloadCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) Downloader.RestartDownload(image); });
		}

		private readonly WindowsApplication application;
		private readonly DataProvider dataProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;

		public string ApplicationTitle
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

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApplicationTitle)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveCollection)));

				SaveCollectionCommand.RaiseCanExecuteChanged();
				ExportCollectionCommand.RaiseCanExecuteChanged();
				CloseCollectionCommand.RaiseCanExecuteChanged();
				SpawnSlideShowCommand.RaiseCanExecuteChanged();
			}
		}

		private Downloader downloaderField;
		public Downloader Downloader
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
		public DelegateCommand<object> ExitApplicationCommand { get; }

		public DelegateCommand<string> CreateCollectionCommand { get; }
		public DelegateCommand<string> LoadCollectionCommand { get; }
		public DelegateCommand<object> SaveCollectionCommand { get; }
		public DelegateCommand<string> ExportCollectionCommand { get; }
		public DelegateCommand<object> CloseCollectionCommand { get; }

		public DelegateCommand<object> SpawnSlideShowCommand { get; }
		public DelegateCommand<ImageViewModel> ViewImageCommand { get; }
		public DelegateCommand<ImageViewModel> OpenImageCommand { get; }
		public DelegateCommand<ImageViewModel> EditImageCommand { get; }
		public DelegateCommand<ImageViewModel> RestartDownloadCommand { get; }

		private void ChangeCollection(CollectionData collectionData, string collectionPath)
		{
			if (ActiveCollection != null)
				CloseCollection();

			CollectionModel collectionModel = new CollectionModel(dataProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(application, collectionModel, queryEngine);
			Downloader = new Downloader(ActiveCollection);
		}

		private void CloseCollection()
		{
			ActiveCollection.Dispose();
			ActiveCollection = null;
			Downloader.Dispose();
			Downloader = null;
		}
	}
}
