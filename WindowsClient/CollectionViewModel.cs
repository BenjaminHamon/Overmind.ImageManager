using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class CollectionViewModel : INotifyPropertyChanged, IDisposable
	{
		public CollectionViewModel(CollectionModel model)
		{
			this.model = model;

			allImages = new ObservableCollection<ImageViewModel>(model.Images.Select(image => new ImageViewModel(image, () => model.GetImagePath(image))));

			RemoveImageCommand = new DelegateCommand<object>(_ => RemoveImage(SelectedImage), _ => SelectedImage != null);
			SearchCommand = new DelegateCommand<string>(Search);
		}

		private readonly CollectionModel model;
		private readonly ObservableCollection<ImageViewModel> allImages;
		private ObservableCollection<ImageViewModel> filteredImages;
		private readonly object modelLock = new object();

		public string Name { get { return model.Name; } }
		public ObservableCollection<ImageViewModel> ImageCollection { get { return filteredImages ?? allImages; } }

		private ImageViewModel selectedImageField;
		public ImageViewModel SelectedImage
		{
			get { return selectedImageField; }
			set
			{
				selectedImageField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));
				RemoveImageCommand.RaiseCanExecuteChanged();
			}
		}

		public void Save()
		{
			lock (modelLock)
			{
				model.Save();
			}
		}

		public void Dispose()
		{
			lock (modelLock)
			{
				model.Dispose();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public DelegateCommand<object> RemoveImageCommand { get; }
		public DelegateCommand<string> SearchCommand { get; }

		public ImageViewModel GetImage(string hash)
		{
			lock (modelLock)
			{
				return allImages.FirstOrDefault(i => i.Hash == hash);
			}
		}

		public ImageViewModel AddImage(string newImageName, byte[] newImageData)
		{
			string newImageHash = ImageModel.CreateHash(newImageData);
			lock (modelLock)
			{
				ImageModel newImage = new ImageModel() { Hash = newImageHash, FileName = newImageName };
				model.AddImage(newImage, newImageData);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.GetImagePath(newImage));
				allImages.Add(newImageViewModel);
				return newImageViewModel;
			}
		}

		private void RemoveImage(ImageViewModel image)
		{
			lock (modelLock)
			{
				model.RemoveImage(image.Model);
				allImages.Remove(image);
				filteredImages?.Remove(image);
			}

			if (SelectedImage == image)
				SelectedImage = null;
		}

		private void Search(string queryString)
		{
			lock (modelLock)
			{
				if (String.IsNullOrEmpty(queryString))
					filteredImages = null;
				else
				{
					Func<ImageModel, bool> queryFunction = model.CreateSearchQuery(queryString);
					IEnumerable<ImageViewModel> matchingImages = allImages.Where(imageViewModel => imageViewModel.IsSearchMatch(queryFunction));
					filteredImages = new ObservableCollection<ImageViewModel>(matchingImages);
				}
			}
			
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageCollection)));
			
			// The memory does not get reliably released when changing the displayed image collection.
			// Manually calling the garbage collector here seems to help reduce the used memory as soon as possible.
			GC.Collect();
		}
	}
}
