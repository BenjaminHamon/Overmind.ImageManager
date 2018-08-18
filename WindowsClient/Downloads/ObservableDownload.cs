using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class ObservableDownload : IDisposable, INotifyPropertyChanged
	{
		public ObservableDownload(string uriString)
		{
			this.uriStringField = uriString;

			CancelCommand = new DelegateCommand<object>(_ => Cancel(), _ => IsDownloading);
		}

		private readonly string uriStringField;
		private readonly object downloadLock = new object();

		private WebClient webClient;

		public Uri Uri { get; private set; }
		public string UriString { get { return Uri == null ? uriStringField : Uri.ToString(); } }
		public string Name { get { return Uri == null ? uriStringField : Uri.UnescapeDataString(Uri.Segments.Last()); } }

		public Action<ObservableDownload, byte[]> DataVerificationHook { get; set; }
		public Action<ObservableDownload, byte[]> DataConsumer { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		public event Action<ObservableDownload, byte[]> DownloadCompleted;

		public bool IsDownloading { get; private set; }
		public bool IsCompleted { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }

		public double TotalSize { get; private set; }
		public double Progress { get; private set; }
		public double ProgressPercentage { get { return TotalSize != 0 ? 100 * Progress / TotalSize : 0; } }

		public DelegateCommand<object> CancelCommand { get; }

		public void Execute()
		{
			lock (downloadLock)
			{
				if (IsDownloading)
					throw new InvalidOperationException("Already executing");
				IsDownloading = true;

				IsCompleted = false;
				Success = false;
				StatusMessage = null;
				TotalSize = 0;
				Progress = 0;

				try
				{
					Uri = new Uri(uriStringField);
				}
				catch (UriFormatException exception)
				{
					IsDownloading = false;
					IsCompleted = true;
					StatusMessage = ExceptionToMessage(exception);

				}

				if (Uri != null)
				{
					webClient = new WebClient();
					webClient.DownloadProgressChanged += HandleDownloadProgress;
					webClient.DownloadDataCompleted += HandleDownloadCompleted;
					webClient.DownloadDataAsync(Uri);
				}
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Uri)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UriString)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Success)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalSize)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercentage)));
			CancelCommand.RaiseCanExecuteChanged();
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

				byte[] downloadData = null;

				try
				{
					if (eventArguments.Error != null)
						throw eventArguments.Error;
					downloadData = eventArguments.Result;
					DataVerificationHook?.Invoke(this, downloadData);
					DataConsumer?.Invoke(this, downloadData);
					Success = true;
				}
				catch (Exception exception)
				{
					StatusMessage = ExceptionToMessage(exception);
				}

				IsCompleted = true;

				DownloadCompleted?.Invoke(this, downloadData);
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Success)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			CancelCommand.RaiseCanExecuteChanged();
		}

		private void Cancel()
		{
			lock (downloadLock)
			{
				if (IsDownloading)
					webClient.CancelAsync();
			}
		}

		private static string ExceptionToMessage(Exception exception)
		{
			string message = null;

			while (exception != null)
			{
				if (message != null)
					message += Environment.NewLine;
				message += exception.Message;
				exception = exception.InnerException;
			}

			return message;
		}
	}
}
