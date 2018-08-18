using Overmind.ImageManager.Model;
using System;
using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImageViewModel : INotifyPropertyChanged
	{
		public ImageViewModel(ImageModel model, Func<string> getImagePath)
		{
			this.Model = model;
			this.getImagePath = getImagePath;
		}

		private readonly Func<string> getImagePath;

		public ImageModel Model { get; }
		public string FilePath { get { return getImagePath(); } }
		public string Name { get { return Model.FileName; } }
		public string Hash { get { return Model.Hash; } }

		public string Group { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyFileChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hash)));
		}
	}
}
