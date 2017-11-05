using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class ImageDownload : INotifyPropertyChanged, IDisposable
	{
		public ImageDownload(Uri uri, CollectionViewModel collection)
		{
			this.Uri = uri;
			this.collection = collection;

			CancelCommand = new DelegateCommand<object>(_ => Cancel(), _ => IsDownloading);
			SelectCommand = new DelegateCommand<object>(_ => Select(), _ => Image != null);
		}

		private readonly CollectionViewModel collection;
		private WebClient webClient;
		private readonly object downloadLock = new object();

		public event PropertyChangedEventHandler PropertyChanged;

		public Uri Uri { get; }
		public string Name { get { return Uri.UnescapeDataString(Uri.Segments.Last()); } }

		public bool IsDownloading { get; private set; }
		public bool Completed { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }

		public double TotalSize { get; private set; }
		public double Progress { get; private set; }
		public double ProgressPercentage { get { return TotalSize != 0 ? 100 * Progress / TotalSize : 0; } }

		public ImageViewModel Image { get; private set; }

		public DelegateCommand<object> CancelCommand { get; }
		public DelegateCommand<object> SelectCommand { get; }

		public void Execute()
		{
			lock (downloadLock)
			{
				if (IsDownloading)
					throw new InvalidOperationException("Already executing");
				IsDownloading = true;

				Completed = false;
				Success = false;
				StatusMessage = null;
				TotalSize = 0;
				Progress = 0;
				Image = null;

				webClient = new WebClient();
				webClient.DownloadProgressChanged += HandleDownloadProgress;
				webClient.DownloadDataCompleted += HandleDownloadCompleted;
				webClient.DownloadDataAsync(Uri);
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Completed)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Success)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalSize)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercentage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
			CancelCommand.RaiseCanExecuteChanged();
			SelectCommand.RaiseCanExecuteChanged();
		}

		public void Dispose()
		{
			Cancel();
		}

		private void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs eventArguments)
		{
			TotalSize = eventArguments.TotalBytesToReceive;
			Progress = eventArguments.BytesReceived;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalSize)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercentage)));
		}

		private void HandleDownloadCompleted(object sender, DownloadDataCompletedEventArgs eventArguments)
		{
			lock (downloadLock)
			{
				IsDownloading = false;
				webClient.Dispose();
				webClient = null;
			}

			if (eventArguments.Error != null)
			{
				StatusMessage = eventArguments.Error.Message;
			}
			else
			{
				Image = collection.GetImage(ImageModel.CreateHash(eventArguments.Result));
				if (Image != null)
				{
					StatusMessage = "Image already exists in the collection";
				}
				else
				{
					Image = collection.AddImage(Name, eventArguments.Result);
					Success = true;
				}
			}

			Completed = true;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Completed)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Success)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
			CancelCommand.RaiseCanExecuteChanged();
			SelectCommand.RaiseCanExecuteChanged();
		}

		private void Cancel()
		{
			lock (downloadLock)
			{
				if (IsDownloading)
					webClient.CancelAsync();
			}
		}

		private void Select()
		{
			collection.SelectedImage = Image;
		}
	}
}
