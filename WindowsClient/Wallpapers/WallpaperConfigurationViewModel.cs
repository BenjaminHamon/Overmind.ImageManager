using Overmind.ImageManager.Model.Wallpapers;
using System;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public class WallpaperConfigurationViewModel : INotifyPropertyChanged
	{
		public WallpaperConfigurationViewModel(WallpaperConfiguration model)
		{
			this.model = model;
		}

		private readonly WallpaperConfiguration model;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Name
		{
			get { return model.Name; }
			set
			{
				if (model.Name == value)
					return;
				model.Name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		public string CollectionPath
		{
			get { return model.CollectionPath; }
			set
			{
				if (model.CollectionPath == value)
					return;
				model.CollectionPath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollectionPath)));
			}
		}

		public string ImageQuery
		{
			get { return model.ImageQuery; }
			set
			{
				if (model.ImageQuery == value)
					return;
				model.ImageQuery = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageQuery)));
			}
		}

		public TimeSpan CyclePeriod
		{
			get { return model.CyclePeriod; }
			set
			{
				if (model.CyclePeriod == value)
					return;
				model.CyclePeriod = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CyclePeriod)));
			}
		}
	}
}
