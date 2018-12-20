using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class Downloader : IDisposable
	{
		public Downloader(CollectionViewModel collection)
		{
			this.collection = collection;

			AddDownloadCommand = new DelegateCommand<string>(AddDownload);
			SelectImageCommand = new DelegateCommand<ObservableDownload>(SelectImage, CanSelectImage);
		}

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
			AddDownload(image.Model.Source.ToString(), (download, downloadData) => collection.UpdateImageFile(image, downloadData));
		}

		private void AddDownload(string uri, Action<ObservableDownload, byte[]> consumer)
		{
			if (String.IsNullOrEmpty(uri))
				return;

			ObservableDownload newDownload = new ObservableDownload(uri)
			{
				DataVerificationHook = VerifyData,
				DataConsumer = consumer,
			};

			DownloadCollection.Add(newDownload);

			newDownload.DownloadCompleted += HandleDownloadCompleted;
			newDownload.Execute();
		}

		private void VerifyData(ObservableDownload download, byte[] downloadData)
		{
			if (IsImage(downloadData) == false)
				throw new InvalidOperationException("The file is not an image");
		}

		private void HandleDownloadCompleted(ObservableDownload download, byte[] downloadData)
		{
			download.DownloadCompleted -= HandleDownloadCompleted;

			if (downloadData != null)
			{
				downloadHashCache[download] = ImageModel.CreateHash(downloadData);
				SelectImageCommand.RaiseCanExecuteChanged();
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
