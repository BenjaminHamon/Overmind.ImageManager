using Overmind.ImageManager.WindowsClient.Extensions;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class SlideShowViewModel : INotifyPropertyChanged
	{
		public SlideShowViewModel(IReadOnlyList<ImageViewModel> imageCollection)
		{
			this.imageCollection = imageCollection;

			isRunningField = CanCycle;
			currentImageField = imageCollection.FirstOrDefault();
			Interval = TimeSpan.FromSeconds(5);

			PreviousCommand = new DelegateCommand<object>(_ => Previous(), _ => CanCycle);
			NextCommand = new DelegateCommand<object>(_ => Next(), _ => CanCycle);
		}

		private readonly IReadOnlyList<ImageViewModel> imageCollection;
		private readonly object cycleLock = new object();

		private bool isRunningField;
		public bool IsRunning
		{
			get
			{
				return isRunningField;
			}
			set
			{
				if (isRunningField == value)
					return;

				isRunningField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
			}
		}

		private ImageViewModel currentImageField;
		public ImageViewModel CurrentImage
		{
			get
			{
				return currentImageField;
			}
			set
			{
				if (currentImageField == value)
					return;

				currentImageField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentImage)));
			}
		}

		public TimeSpan Interval { get; private set; }
		public double IntervalSeconds
		{
			get
			{
				return Interval.TotalSeconds;
			}
			set
			{
				TimeSpan valueTimeSpan = TimeSpan.FromSeconds(value);
				if (valueTimeSpan == Interval)
					return;

				Interval = valueTimeSpan;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interval)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IntervalSeconds)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public DelegateCommand<object> PreviousCommand { get; }
		public DelegateCommand<object> NextCommand { get; }

		public bool CanCycle { get { return imageCollection.Count > 1; } }

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
