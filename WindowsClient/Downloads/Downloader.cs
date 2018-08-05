using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
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

		public DelegateCommand<string> AddDownloadCommand { get; }
		public DelegateCommand<ObservableDownload> SelectImageCommand { get; }

		public void Dispose()
		{
			foreach (ObservableDownload download in DownloadCollection)
				download.Dispose();
		}

		public void AddDownload(string uri)
		{
			if (String.IsNullOrEmpty(uri))
				return;

			ObservableDownload newDownload = new ObservableDownload(uri)
			{
				DataVerificationHook = VerifyData,
			};

			DownloadCollection.Add(newDownload);

			newDownload.DownloadCompleted += HandleDownloadCompleted;
			newDownload.Execute();
		}

		private void VerifyData(byte[] downloadData)
		{
			if (IsImage(downloadData) == false)
				throw new InvalidOperationException("The file is not an image");

			string hash = ImageModel.CreateHash(downloadData);
			ImageViewModel existingImage = collection.GetImage(hash);
			if (existingImage != null)
				throw new InvalidOperationException("The image already exists in the collection");
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

		private void HandleDownloadCompleted(ObservableDownload download)
		{
			download.DownloadCompleted -= HandleDownloadCompleted;

			if (download.Success)
				collection.AddImage(download.Name, download.Uri, download.DownloadedData);

			SelectImageCommand.RaiseCanExecuteChanged();
		}

		private bool CanSelectImage(ObservableDownload download)
		{
			return download.DownloadedData != null;
		}

		private void SelectImage(ObservableDownload download)
		{
			if (download.DownloadedData == null)
				return;

			string hash = ImageModel.CreateHash(download.DownloadedData);
			collection.SelectedImage = collection.GetImage(hash);
		}
	}
}
