using NLog;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Downloads;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class DownloaderViewModel : IDisposable
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(DownloaderViewModel));

		private delegate void DownloadConsumer(ObservableDownload download, byte[] downloadData);

		public DownloaderViewModel(IDownloader downloader, CollectionViewModel collection, SettingsProvider settingsProvider, Dispatcher dispatcher)
		{
			this.downloader = downloader;
			this.collection = collection;
			this.settingsProvider = settingsProvider;
			this.dispatcher = dispatcher;

			AddDownloadCommand = new DelegateCommand<string>(AddDownload);
			SelectImageCommand = new DelegateCommand<ObservableDownload>(SelectImage, CanSelectImage);
		}

		private readonly Dispatcher dispatcher;
		private readonly IDownloader downloader;
		private readonly CollectionViewModel collection;
		private readonly SettingsProvider settingsProvider;

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

				uri = await ResolveUri(uri, cancellationTokenSource.Token);

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

		private async Task<Uri> ResolveUri(Uri uri, CancellationToken cancellationToken)
		{
			if ((uri.Scheme == "http") || (uri.Scheme == "https"))
			{
				DownloadSourceConfiguration sourceConfiguration = TryLoadSettings()
					.SourceConfigurationCollection.FirstOrDefault(configuration => configuration.DomainName == uri.Host);

				if (sourceConfiguration != null)
				{
					try
					{
						if (String.IsNullOrEmpty(sourceConfiguration.ApiUriFormat) == false)
						{
							Uri apiUri = new Uri(uri, String.Format(sourceConfiguration.ApiUriFormat, uri.Segments));
							return await downloader.ResolveUriFromWebApi(apiUri, sourceConfiguration.ApiResponsePath, cancellationToken);
						}

						if (String.IsNullOrEmpty(sourceConfiguration.XPath) == false)
						{
							return await downloader.ResolveUriFromWebPage(uri, sourceConfiguration.XPath, cancellationToken);
						}
					}
					catch (InvalidDataException)
					{
						return uri;
					}
				}
			}

			return uri;
		}

		private DownloaderSettings TryLoadSettings()
		{
			DownloaderSettings downloaderSettings = null;

			try
			{
				downloaderSettings = settingsProvider.LoadApplicationSettings().DownloaderSettings
					?? throw new ArgumentNullException(nameof(downloaderSettings), "Downloader settings must not be null");
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load downloader settings");
			}

			return downloaderSettings ?? new DownloaderSettings();
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
