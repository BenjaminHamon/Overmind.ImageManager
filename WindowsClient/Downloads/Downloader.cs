using Overmind.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class Downloader : IDisposable, INotifyPropertyChanged
	{
		public Downloader(CollectionViewModel collection)
		{
			this.collection = collection;

			AddDownloadCommand = new DelegateCommand<object>(_ => AddDownload(), _ => String.IsNullOrEmpty(AddDownloadUri) == false);
		}

		private readonly CollectionViewModel collection;

		public event PropertyChangedEventHandler PropertyChanged;

		private string addDownloadUriField;
		public string AddDownloadUri
		{
			get { return addDownloadUriField; }
			set
			{
				if (addDownloadUriField == value)
					return;
				addDownloadUriField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddDownloadUri)));
				AddDownloadCommand.RaiseCanExecuteChanged();
			}
		}

		public ObservableCollection<ImageDownload> DownloadCollection { get; } = new ObservableCollection<ImageDownload>();

		public DelegateCommand<object> AddDownloadCommand { get; }

		public void Dispose()
		{
			foreach (ImageDownload download in DownloadCollection)
				download.Dispose();
		}

		private void AddDownload()
		{
			ImageDownload newDownload = new ImageDownload(AddDownloadUri, collection);
			DownloadCollection.Add(newDownload);
			newDownload.Execute();
		}
	}
}
