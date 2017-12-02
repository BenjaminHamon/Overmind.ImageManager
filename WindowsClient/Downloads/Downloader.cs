using Overmind.WpfExtensions;
using System;
using System.Collections.ObjectModel;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class Downloader : IDisposable
	{
		public Downloader(CollectionViewModel collection)
		{
			this.collection = collection;

			AddDownloadCommand = new DelegateCommand<string>(uri => AddDownload(uri));
		}

		private readonly CollectionViewModel collection;

		public ObservableCollection<ImageDownload> DownloadCollection { get; } = new ObservableCollection<ImageDownload>();

		public DelegateCommand<string> AddDownloadCommand { get; }

		public void Dispose()
		{
			foreach (ImageDownload download in DownloadCollection)
				download.Dispose();
		}

		public void AddDownload(string uri)
		{
			if (String.IsNullOrEmpty(uri))
				return;

			ImageDownload newDownload = new ImageDownload(uri, collection);
			DownloadCollection.Add(newDownload);
			newDownload.Execute();
		}
	}
}
