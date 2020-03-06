using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class ObservableDownload : INotifyPropertyChanged, IDisposable
	{
		public ObservableDownload(string uriString)
		{
			this.uriStringField = uriString;

			CancelCommand = new DelegateCommand<object>(_ => Cancel(), _ => IsDownloading);
		}

		private readonly string uriStringField;
		private CancellationTokenSource cancellationTokenSource;

		public Uri Uri { get; private set; }
		public string UriString { get { return Uri == null ? uriStringField : Uri.ToString(); } }
		public string Name { get { return Uri == null ? uriStringField : Uri.UnescapeDataString(Uri.Segments.Last()); } }

		public bool IsDownloading { get; private set; }
		public bool IsCompleted { get; private set; }
		public bool Success { get; private set; }
		public Exception Exception { get; private set; }
		public string StatusMessage { get; private set; }

		public long TotalSize { get; private set; }
		public long Progress { get; private set; }
		public double ProgressPercentage { get { return TotalSize != 0 ? 100 * Convert.ToDouble(Progress) / TotalSize : 0; } }

		public event PropertyChangedEventHandler PropertyChanged;
		public DelegateCommand<object> CancelCommand { get; }

		public void Reset()
		{
			IsDownloading = false;
			IsCompleted = false;
			Success = false;

			Exception = null;
			StatusMessage = null;

			Progress = 0;
			TotalSize = 0;

			PropertyChanged?.Invoke(this, null);
		}

		public void Start(Uri uri, CancellationTokenSource cancellationTokenSource)
		{
			this.Uri = uri;
			this.cancellationTokenSource = cancellationTokenSource;

			IsDownloading = true;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Uri)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UriString)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			CancelCommand.RaiseCanExecuteChanged();
		}

		public void UpdateProgress(long progress, long totalSize)
		{
			this.Progress = progress;
			this.TotalSize = totalSize;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalSize)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercentage)));
		}

		public void Complete(Exception exception)
		{
			this.cancellationTokenSource?.Dispose();
			this.cancellationTokenSource = null;

			this.Exception = exception;

			IsDownloading = false;
			IsCompleted = true;
			Success = exception == null;

			if (exception != null)
				StatusMessage = FormatExtensions.FormatExceptionSummary(exception);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Success)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exception)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			CancelCommand.RaiseCanExecuteChanged();
		}

		public void Cancel()
		{
			this.cancellationTokenSource.Cancel();
		}

		public void Dispose()
		{
			this.cancellationTokenSource?.Cancel();
			this.cancellationTokenSource?.Dispose();
		}
	}
}
