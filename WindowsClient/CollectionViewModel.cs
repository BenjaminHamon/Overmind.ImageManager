using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient
{
	public class CollectionViewModel : INotifyPropertyChanged
	{
		public CollectionViewModel(WindowsApplication application, CollectionModel model,
			IImageOperations imageOperations, IQueryEngine<ImageModel> queryEngine, Func<Random> randomFactory)
		{
			this.model = model;
			this.imageOperations = imageOperations;
			this.dispatcher = application.Dispatcher;

			allImages = new List<ImageViewModel>();

			foreach (ImageModel image in model.AllImages)
			{
				allImages.Add(new ImageViewModel(image, () => model.GetImagePath(image)));
			}

			Query = new QueryViewModel(queryEngine, randomFactory);
			FilteredImages = new List<ImageViewModel>();
			DisplayedImages = new ObservableCollection<ImageViewModel>();

			ViewImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) application.ViewImage(image); });
			RemoveImageCommand = new DelegateCommand<ImageViewModel>(image => { if (image != null) RemoveImage(image); });
			ExecuteQueryCommand = new DelegateCommand<object>(_ => ExecuteQuery());
		}

		private readonly CollectionModel model;
		private readonly List<ImageViewModel> allImages;
		private readonly IImageOperations imageOperations;
		private readonly Dispatcher dispatcher;
		private readonly object modelLock = new object();

		public string Name { get { return Path.GetFileName(model.StoragePath); } }
		public string StoragePath { get { return model.StoragePath; } }
		public QueryViewModel Query { get; }
		public List<ImageViewModel> FilteredImages { get; private set; }

		// Images are added to display only when requested by the view, to improve memory usage and loading speed.
		// This also reduces the lag when the underlying collection changes while previous one is being loaded:
		// the image loading cannot be cancelled and will finish before being discarded and before the loading for the new images starts.
		// A virtualized panel in the view helps as well, by keeping loaded only the images which are actually visible.
		public ObservableCollection<ImageViewModel> DisplayedImages { get; }

		private ImageViewModel selectedImageField;
		public ImageViewModel SelectedImage
		{
			get
			{
				return selectedImageField;
			}
			set
			{
				// Reset selection in case the image is not in DisplayedImages
				// to ensure view elements do not keep the old selection value
				selectedImageField = null;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));

				selectedImageField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));

				SelectedImageProperties = selectedImageField == null ? null
					: new ImagePropertiesViewModel(selectedImageField.Model, () => model.GetImagePath(selectedImageField.Model), imageOperations, dispatcher)
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

		public bool IsSaved()
		{
			lock (modelLock)
			{
				return model.IsSaved();
			}
		}

		public void Export(string destinationPath)
		{
			lock (modelLock)
			{
				model.Export(destinationPath, FilteredImages.Select(image => image.Model));
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

		public ImageViewModel AddImage(Uri source, byte[] data)
		{
			lock (modelLock)
			{
				ImageModel newImage = model.CreateImage(source, data);
				ImageViewModel newImageViewModel = new ImageViewModel(newImage, () => model.GetImagePath(newImage)) { Group = "#New" };
				allImages.Insert(0, newImageViewModel);
				FilteredImages.Insert(0, newImageViewModel);
				DisplayedImages.Insert(0, newImageViewModel);
				return newImageViewModel;
			}
		}

		public void UpdateImageFile(ImageViewModel image, byte[] data)
		{
			lock (modelLock)
			{
				model.UpdateImageFile(image.Model, data);

				image.NotifyFileChanged();

				if (SelectedImage == image)
				{
					SelectedImageProperties.NotifyFileChanged();
				}
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
			Query.Validate();
			if (Query.HasErrors)
				return;

			lock (modelLock)
			{
				FilteredImages = Query.Execute(allImages).ToList();
			}

			ResetDisplay();

			// The memory does not get reliably released when changing the displayed image collection.
			// Manually calling the garbage collector here seems to help reduce the used memory as soon as possible.
			GC.Collect();
		}

		public bool CanDisplayMore { get { return DisplayedImages.Count != (FilteredImages ?? allImages).Count; } }

		public void DisplayMore()
		{
			IEnumerable<ImageViewModel> imagesToAdd = (FilteredImages ?? allImages).Skip(DisplayedImages.Count).Take(50);

			foreach (ImageViewModel image in imagesToAdd)
			{
				DisplayedImages.Add(image);
			}
		}

		public void ResetDisplay()
		{
			DisplayedImages.Clear();
		}
	}
}
