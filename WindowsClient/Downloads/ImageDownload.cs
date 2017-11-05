using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class ImageDownload : IDisposable, INotifyPropertyChanged
	{
		public ImageDownload(string uriString, CollectionViewModel collection)
		{
			this.uriStringField = uriString;
			this.collection = collection;

			if ((uriString != null) && Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
			{
				Uri uri = new Uri(uriString);
				IEnumerable<char> invalidCharacters = Path.GetInvalidFileNameChars();
				if (Uri.UnescapeDataString(uri.Segments.Last()).Any(c => invalidCharacters.Contains(c) == false))
					this.uri = uri;
			}
			
			CancelCommand = new DelegateCommand<object>(_ => Cancel(), _ => IsDownloading);
			SelectCommand = new DelegateCommand<object>(_ => Select(), _ => Image != null);
		}

		private readonly string uriStringField;
		private readonly Uri uri;
		private readonly CollectionViewModel collection;
		private WebClient webClient;
		private readonly object downloadLock = new object();

		public event PropertyChangedEventHandler PropertyChanged;
		
		public string UriString { get { return uri == null ? uriStringField : uri.ToString(); } }
		public string Name { get { return uri == null ? UriString : Uri.UnescapeDataString(uri.Segments.Last()); } }

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

				if (uri == null)
				{
					IsDownloading = false;
					Completed = true;
					StatusMessage = "Invalid URI";
				}
				else
				{
					webClient = new WebClient();
					webClient.DownloadProgressChanged += HandleDownloadProgress;
					webClient.DownloadDataCompleted += HandleDownloadCompleted;
					webClient.DownloadDataAsync(uri);
				}
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
				Exception exception = eventArguments.Error;
				while (exception != null)
				{
					if (StatusMessage != null)
						StatusMessage += Environment.NewLine;
					StatusMessage += exception.Message;
					exception = exception.InnerException;
				}
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
