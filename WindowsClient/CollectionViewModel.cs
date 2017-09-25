using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

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
				ImageModel newImage = new ImageModel() { Hash = newImageHash, FileName = uri.Segments.Last() };
				model.AddImage(newImage, newImageData);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.GetImagePath(newImage));
				allImages.Add(newImageViewModel);
				SelectedImage = newImageViewModel;
			}
		}

		private void Search(string query)
		{
			if (String.IsNullOrEmpty(query))
				filteredImages = null;
			else
			{
				List<Regex> queryRegexes = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(element => new Regex("^" + Regex.Escape(element).Replace("\\*", ".*") + "$")).ToList();
				IEnumerable<ImageViewModel> matchingImages = allImages
					.Where(image => queryRegexes.All(regex => image.GetSearchableValues().Any(value => regex.IsMatch(value))));
				filteredImages = new ObservableCollection<ImageViewModel>(matchingImages);
			}
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageCollection)));
		}
	}
}
