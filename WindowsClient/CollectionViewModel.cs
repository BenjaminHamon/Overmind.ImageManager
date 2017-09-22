using Overmind.ImageManager.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Overmind.ImageManager.WindowsClient
{
	public class CollectionViewModel : INotifyPropertyChanged, IDisposable
	{
		public CollectionViewModel(CollectionModel model)
		{
			this.model = model;

			ImageCollection = new ObservableCollection<ImageViewModel>(model.Images.Select(image => new ImageViewModel(image, () => model.StoragePath)));

			AddImageCommand = new DelegateCommand<string>(uri => AddImage(new Uri(uri)));
		}

		private readonly CollectionModel model;

		public string Name { get { return model.Name; } }
		public ObservableCollection<ImageViewModel> ImageCollection { get; }

		private ImageViewModel selectedImageField;
		public ImageViewModel SelectedImage
		{
			get { return selectedImageField; }
			set
			{
				if (value == selectedImageField)
					return;
				selectedImageField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));
			}
		}

		public void Save()
		{
			model.Save();
		}

		public void Dispose()
		{
			model.Dispose();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public DelegateCommand<string> AddImageCommand { get; }

		private void AddImage(Uri uri)
		{
			byte[] newImageData;
			using (WebClient webClient = new WebClient())
				newImageData = webClient.DownloadData(uri);

			string newImageHash = ImageModel.CreateHash(newImageData);
			ImageViewModel existingImage = ImageCollection.FirstOrDefault(i => i.Hash == newImageHash);

			if (existingImage != null)
			{
				SelectedImage = existingImage;
			}
			else
			{
				ImageModel newImage = new ImageModel() { Hash = newImageHash, FileName = uri.Segments.Last() };
				model.AddImage(newImage, newImageData);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.StoragePath);
				ImageCollection.Add(newImageViewModel);
				SelectedImage = newImageViewModel;
			}
		}
	}
}
