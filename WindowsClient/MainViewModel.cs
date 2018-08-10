using Overmind.ImageManager.Model;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class MainViewModel : INotifyPropertyChanged, IDisposable
	{
		public MainViewModel(DataProvider dataProvider)
		{
			this.dataProvider = dataProvider;

			CreateCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(dataProvider.CreateCollection(path), path));
			LoadCollectionCommand = new DelegateCommand<string>(path => ChangeCollection(dataProvider.LoadCollection(path), path));
			SaveCollectionCommand = new DelegateCommand<object>(_ => ActiveCollection.Save(), _ => ActiveCollection != null);
			CloseCollectionCommand = new DelegateCommand<object>(_ => CloseCollection(), _ => ActiveCollection != null);
		}

		private readonly DataProvider dataProvider;

		public string ApplicationTitle
		{
			get
			{
				if (ActiveCollection == null)
					return WindowsApplication.Name;
				return ActiveCollection.Name + " - " + WindowsApplication.Name;
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
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanExportQuery)));

				SaveCollectionCommand.RaiseCanExecuteChanged();
				CloseCollectionCommand.RaiseCanExecuteChanged();
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
		public DelegateCommand<string> CreateCollectionCommand { get; }
		public DelegateCommand<string> LoadCollectionCommand { get; }
		public DelegateCommand<object> SaveCollectionCommand { get; }
		public DelegateCommand<object> CloseCollectionCommand { get; }

		private void ChangeCollection(CollectionData collectionData, string collectionPath)
		{
			if (ActiveCollection != null)
				CloseCollection();

			CollectionModel collectionModel = new CollectionModel(dataProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(collectionModel);
			Downloader = new Downloader(ActiveCollection);
		}

		private void CloseCollection()
		{
			ActiveCollection.Dispose();
			ActiveCollection = null;
			Downloader.Dispose();
			Downloader = null;
		}

		public bool CanExportQuery { get { return ActiveCollection != null; } }

		public void ExportQuery(string collectionPath)
		{
			CollectionData collectionData = new CollectionData();
			collectionData.Images = ActiveCollection.FilteredImages.Select(image => image.Model).ToList();
			dataProvider.ExportCollection(ActiveCollection.StoragePath, collectionPath, collectionData);
		}
	}
}
