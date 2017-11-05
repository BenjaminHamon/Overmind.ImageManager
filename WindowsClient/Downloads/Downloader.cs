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

			AddDownloadCommand = new DelegateCommand<string>(AddDownload);
		}

		private readonly CollectionViewModel collection;

		public ObservableCollection<ImageDownload> DownloadCollection { get; } = new ObservableCollection<ImageDownload>();

		public DelegateCommand<string> AddDownloadCommand { get; }

		public void Dispose()
		{
			foreach (ImageDownload download in DownloadCollection)
				download.Dispose();
		}

		private void AddDownload(string uriString)
		{
			Uri uri;
			try { uri = new Uri(uriString); }
			catch { return; }

			ImageDownload newDownload = new ImageDownload(uri, collection);
			DownloadCollection.Add(newDownload);
			newDownload.Execute();
		}
	}
}
