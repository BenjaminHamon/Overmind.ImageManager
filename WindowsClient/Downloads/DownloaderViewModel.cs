using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Downloads;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class DownloaderViewModel : IDisposable
	{
		private delegate void DownloadConsumer(ObservableDownload download, byte[] downloadData);

		public DownloaderViewModel(IDownloader downloader, CollectionViewModel collection, Dispatcher dispatcher)
		{
			this.downloader = downloader;
			this.collection = collection;
			this.dispatcher = dispatcher;

			AddDownloadCommand = new DelegateCommand<string>(AddDownload);
			SelectImageCommand = new DelegateCommand<ObservableDownload>(SelectImage, CanSelectImage);
		}

		private readonly Dispatcher dispatcher;
		private readonly IDownloader downloader;
		private readonly CollectionViewModel collection;

		public ObservableCollection<ObservableDownload> DownloadCollection { get; } = new ObservableCollection<ObservableDownload>();
		private Dictionary<ObservableDownload, string> downloadHashCache = new Dictionary<ObservableDownload, string>();

		public DelegateCommand<string> AddDownloadCommand { get; }
		public DelegateCommand<ObservableDownload> SelectImageCommand { get; }

		public void Dispose()
		{
			foreach (ObservableDownload download in DownloadCollection)
				download.Dispose();
		}

		public void AddDownload(string uri)
		{
			AddDownload(uri, (download, downloadData) => collection.AddImage(download.Uri, downloadData));
		}

		public void RestartDownload(ImageViewModel image)
		{
			AddDownload(image.Model.Source?.ToString(), (download, downloadData) => collection.UpdateImageFile(image, downloadData));
		}

		private void AddDownload(string uri, DownloadConsumer consumer)
		{
			if (String.IsNullOrEmpty(uri))
				return;

			ObservableDownload download = new ObservableDownload(uri);

			DownloadCollection.Add(download);

			Task.Run(() => DownloadAsync(download, uri, consumer));
		}

		private async Task DownloadAsync(ObservableDownload download, string uriString, DownloadConsumer consumer)
		{
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			Exception downloadException = null;
			byte[] downloadData = null;

			try
			{
				Uri uri = new Uri(uriString, UriKind.Absolute);

				dispatcher.Invoke(() => download.Start(uri, cancellationTokenSource));

				ProgressHandler progressHandler = (_, progress, totalSize) => dispatcher.Invoke(() => download.UpdateProgress(progress, totalSize));
				downloadData = await downloader.Download(uri, progressHandler, cancellationTokenSource.Token);

				if (IsImage(downloadData) == false)
					throw new InvalidOperationException("The file is not an image");

				if (downloadData != null)
				{
					dispatcher.Invoke(() =>
					{
						try
						{
							consumer.Invoke(download, downloadData);
						}
						catch (Exception exception)
						{
							downloadException = exception;
						}
					});
				}
			}
			catch (Exception exception)
			{
				downloadException = exception;
			}

			dispatcher.Invoke(() => download.Complete(downloadException));

			if (downloadData != null)
			{
				dispatcher.Invoke(() =>
				{
					downloadHashCache[download] = ImageModel.CreateHash(downloadData);
					SelectImageCommand.RaiseCanExecuteChanged();
				});
			}
		}

		private bool IsImage(byte[] data)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(data))
				using (Image imageObject = Image.FromStream(stream))
					return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		private bool CanSelectImage(ObservableDownload download)
		{
			return downloadHashCache.ContainsKey(download);
		}

		private void SelectImage(ObservableDownload download)
		{
			collection.SelectedImage = collection.GetImage(downloadHashCache[download]);
		}
	}
}
