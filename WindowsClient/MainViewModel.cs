using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;

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
					return "Overmind Image Manager";
				return ActiveCollection.Name + " - Overmind Image Manager";
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
				CloseCollectionCommand.RaiseCanExecuteChanged();
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

		private void CreateCollection(string collectionPath)
		{
			CollectionData collectionData = dataProvider.CreateCollection(collectionPath);
			CollectionModel collectionModel = new CollectionModel(dataProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(collectionModel);
		}

		private void LoadCollection(string collectionPath)
		{
			CollectionData collectionData = dataProvider.LoadCollection(collectionPath);
			CollectionModel collectionModel = new CollectionModel(dataProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(collectionModel);
		}

		private void ChangeCollection(CollectionData collectionData, string collectionPath)
		{
			if (ActiveCollection != null)
				CloseCollection();

			CollectionModel collectionModel = new CollectionModel(dataProvider, collectionData, collectionPath);
			ActiveCollection = new CollectionViewModel(collectionModel);
		}

		private void CloseCollection()
		{
			CollectionViewModel previousCollection = ActiveCollection;
			ActiveCollection = null;
			previousCollection.Dispose();
		}
	}
}
