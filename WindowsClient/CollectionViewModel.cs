using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
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

			allImages = new ObservableCollection<ImageViewModel>(model.Images.Select(image => new ImageViewModel(image, () => model.GetImagePath(image))));

			AddImageCommand = new DelegateCommand<string>(uri => AddImage(new Uri(uri)));
			SearchCommand = new DelegateCommand<string>(Search);
		}

		private readonly CollectionModel model;
		private readonly ObservableCollection<ImageViewModel> allImages;
		private ObservableCollection<ImageViewModel> filteredImages;

		public string Name { get { return model.Name; } }
		public ObservableCollection<ImageViewModel> ImageCollection { get { return filteredImages ?? allImages; } }

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
		public DelegateCommand<string> SearchCommand { get; }

		private void AddImage(Uri uri)
		{
			byte[] newImageData;
			using (WebClient webClient = new WebClient())
				newImageData = webClient.DownloadData(uri);

			string newImageHash = ImageModel.CreateHash(newImageData);
			ImageViewModel existingImage = allImages.FirstOrDefault(i => i.Hash == newImageHash);

			if (existingImage != null)
			{
				SelectedImage = existingImage;
			}
			else
			{
				ImageModel newImage = new ImageModel() { Hash = newImageHash, FileName = Uri.UnescapeDataString(uri.Segments.Last()) };
				model.AddImage(newImage, newImageData);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.GetImagePath(newImage));
				allImages.Add(newImageViewModel);
				SelectedImage = newImageViewModel;
			}
		}

		private void Search(string queryString)
		{
			if (String.IsNullOrEmpty(queryString))
				filteredImages = null;
			else
			{
				Func<ImageModel, bool> queryFunction = model.CreateSearchQuery(queryString);
				IEnumerable<ImageViewModel> matchingImages = allImages.Where(imageViewModel => imageViewModel.IsSearchMatch(queryFunction));
				filteredImages = new ObservableCollection<ImageViewModel>(matchingImages);
			}
			
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageCollection)));
			
			// The memory does not get reliably released when changing the displayed image collection.
			// Manually calling the garbage collector here seems to help reduce the used memory as soon as possible.
			GC.Collect();
		}
	}
}
