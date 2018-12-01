using Overmind.ImageManager.Model.Wallpapers;
using System;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public class WallpaperConfigurationViewModel : INotifyPropertyChanged
	{
		public WallpaperConfigurationViewModel(WallpaperConfiguration model)
		{
			this.Model = model;
		}

		public WallpaperConfiguration Model { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		public string Name
		{
			get { return Model.Name; }
			set
			{
				if (Model.Name == value)
					return;
				Model.Name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		public string CollectionPath
		{
			get { return Model.CollectionPath; }
			set
			{
				if (Model.CollectionPath == value)
					return;
				Model.CollectionPath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollectionPath)));
			}
		}

		public string ImageQuery
		{
			get { return Model.ImageQuery; }
			set
			{
				if (Model.ImageQuery == value)
					return;
				Model.ImageQuery = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageQuery)));
			}
		}

		public TimeSpan CyclePeriod
		{
			get { return Model.CyclePeriod; }
			set
			{
				if (Model.CyclePeriod == value)
					return;
				Model.CyclePeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CyclePeriod)));
			}
		}
	}
}
