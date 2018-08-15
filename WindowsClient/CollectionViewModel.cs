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

			allImages = new List<ImageViewModel>(model.AllImages.Select(image => new ImageViewModel(image, () => model.GetImagePath(image))));
			Query = new CollectionQuery();
			FilteredImages = new List<ImageViewModel>(allImages);
			DisplayedImages = new ObservableCollection<ImageViewModel>();

			ExecuteQuery();

			ViewImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) WindowsApplication.ViewImage(image); });
			RemoveImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) RemoveImage(image); });
			ExecuteQueryCommand = new DelegateCommand<object>(_ => ExecuteQuery());
		}

		private readonly CollectionModel model;
		private readonly List<ImageViewModel> allImages;
		private readonly object modelLock = new object();

		public string Name { get { return model.StoragePath; } }
		public string StoragePath { get { return model.StoragePath; } }
		public CollectionQuery Query { get; }
		public List<ImageViewModel> FilteredImages { get; private set; }

		// Images are added to display only when requested by the view, to improve memory usage and loading speed.
		// This also reduces the lag when the underlying collection changes while previous one is being loaded:
		// the image loading cannot be cancelled and will finish before being discarded and before the loading for the new images starts.
		// A virtualized panel in the view helps as well, by keeping loaded only the images which are actually visible.
		public ObservableCollection<ImageViewModel> DisplayedImages { get; }

		private ImageViewModel selectedImageField;
		public ImageViewModel SelectedImage
		{
			get { return selectedImageField; }
			set
			{
				selectedImageField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));

				SelectedImageProperties = selectedImageField == null ? null
					: new ImagePropertiesViewModel(selectedImageField.Model, () => model.GetImagePath(selectedImageField.Model))
				{
					AllSubjects = model.AllImages.SelectMany(image => image.SubjectCollection).Distinct().OrderBy(x => x).ToList(),
					AllArtists = model.AllImages.SelectMany(image => image.ArtistCollection).Distinct().OrderBy(x => x).ToList(),
					AllTags = model.AllImages.SelectMany(image => image.TagCollection).Distinct().OrderBy(x => x).ToList(),
				};
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImageProperties)));
			}
		}

		public ImagePropertiesViewModel SelectedImageProperties { get; private set; }

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
		public DelegateCommand<ImageViewModel> ViewImageCommand { get; }
		public DelegateCommand<ImageViewModel> RemoveImageCommand { get; }
		public DelegateCommand<object> ExecuteQueryCommand { get; }

		public ImageViewModel GetImage(string hash)
		{
			lock (modelLock)
			{
				return allImages.FirstOrDefault(i => i.Hash == hash);
			}
		}

		public ImageViewModel AddImage(string fileName, Uri source, byte[] data)
		{
			string hash = ImageModel.CreateHash(data);
			lock (modelLock)
			{
				ImageModel newImage = new ImageModel() { Hash = hash, FileName = fileName, AdditionDate = DateTime.Now, Source = source };
				model.AddImage(newImage, data);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.GetImagePath(newImage)) { Group = "#New" };
				allImages.Insert(0, newImageViewModel);
				FilteredImages.Insert(0, newImageViewModel);
				DisplayedImages.Insert(0, newImageViewModel);
				return newImageViewModel;
			}
		}

		private void RemoveImage(ImageViewModel image)
		{
			lock (modelLock)
			{
				model.RemoveImage(image.Model);
				allImages.Remove(image);
				FilteredImages?.Remove(image);
				DisplayedImages.Remove(image);
			}

			if (SelectedImage == image)
				SelectedImage = null;
		}

		private void ExecuteQuery()
		{
			lock (modelLock)
			{
				Query.Error = null;

				try
				{
					IEnumerable<ImageViewModel> resultImages = Query.Execute(model, allImages);
					FilteredImages = new List<ImageViewModel>(resultImages);
				}
				catch (Exception exception)
				{
					Query.Error = exception.Message;
				}
			}

			if (Query.Error == null)
			{
				ResetDisplay();

				// The memory does not get reliably released when changing the displayed image collection.
				// Manually calling the garbage collector here seems to help reduce the used memory as soon as possible.
				GC.Collect();
			}
		}

		public bool CanDisplayMore { get { return DisplayedImages.Count != (FilteredImages ?? allImages).Count; } }

		public void DisplayMore()
		{
			IEnumerable<ImageViewModel> imagesToAdd = (FilteredImages ?? allImages).Skip(DisplayedImages.Count).Take(50);
			foreach (ImageViewModel image in imagesToAdd)
				DisplayedImages.Add(image);
		}

		public void ResetDisplay()
		{
			DisplayedImages.Clear();
		}
	}
}
