using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Overmind.ImageManager.WindowsClient
{
	public class SlideShowViewModel : INotifyPropertyChanged, IDisposable
	{
		public SlideShowViewModel(IEnumerable<ImageViewModel> imageCollection, Random random)
		{
			this.imageCollection = imageCollection.Shuffle(random).ToList();

			isRunningField = true;
			currentImageField = this.imageCollection.FirstOrDefault();
			interval = TimeSpan.FromSeconds(5);
			cycleTimer = new Timer(_ => Next(), null, interval, interval);
			
			PreviousCommand = new DelegateCommand<object>(_ => Previous(), _ => this.imageCollection.Count > 1);
			NextCommand = new DelegateCommand<object>(_ => Next(), _ => this.imageCollection.Count > 1);
		}

		private readonly List<ImageViewModel> imageCollection;
		private readonly Timer cycleTimer;
		private readonly object cycleLock = new object();

		private bool isRunningField;
		public bool IsRunning
		{
			get { return isRunningField; }
			set
			{
				if (isRunningField == value)
					return;
				cycleTimer.Change(value ? interval : Timeout.InfiniteTimeSpan, interval);
				isRunningField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
			}
		}

		private ImageViewModel currentImageField;
		public ImageViewModel CurrentImage
		{
			get { return currentImageField; }
			set
			{
				if (currentImageField == value)
					return;
				currentImageField = value;
				if (IsRunning)
					cycleTimer.Change(interval, interval);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentImage)));
			}
		}

		private TimeSpan interval;
		public double IntervalSeconds
		{
			get { return interval.TotalSeconds; }
			set
			{
				TimeSpan valueTimeSpan = TimeSpan.FromSeconds(value);
				if (IsRunning)
					cycleTimer.Change(valueTimeSpan, valueTimeSpan);
				interval = valueTimeSpan;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IntervalSeconds)));
			}
		}

		public void Dispose()
		{
			cycleTimer.Dispose();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public DelegateCommand<object> PreviousCommand { get; }
		public DelegateCommand<object> NextCommand { get; }

		private void Previous()
		{
			lock (cycleLock)
			{
				CurrentImage = CurrentImage == imageCollection.First() ?
					imageCollection.Last() : imageCollection[imageCollection.IndexOf(CurrentImage) - 1];
			}
		}

		private void Next()
		{
			lock (cycleLock)
			{
				CurrentImage = CurrentImage == imageCollection.Last() ?
					CurrentImage = imageCollection.First() : imageCollection[imageCollection.IndexOf(CurrentImage) + 1];
			}
		}
	}
}
