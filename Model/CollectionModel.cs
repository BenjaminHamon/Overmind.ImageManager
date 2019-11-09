using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionModel : ReadOnlyCollectionModel
	{
		public CollectionModel(ICollectionProvider collectionProvider, CollectionData data, string storagePath)
			: base(collectionProvider, data, storagePath)
		{ }

		private readonly List<ImageModel> removedImages = new List<ImageModel>();

		public ImageModel CreateImage(Uri source, byte[] imageData)
		{
			string hash = ImageModel.CreateHash(imageData);
			DateTime now = DateTime.UtcNow;

			// Change the datetime resolution to seconds
			now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));

			ImageModel newImage = new ImageModel() { Hash = hash, AdditionDate = now, Source = source };
			if (data.Images.Any(image => image.Hash == newImage.Hash))
				throw new InvalidOperationException("An image with the same hash already exists in the collection");

			collectionProvider.WriteImageFile(storagePath, newImage, imageData);
			data.Images.Add(newImage);

			return newImage;
		}

		public void UpdateImageFile(ImageModel imageToUpdate, byte[] imageData)
		{
			string hash = ImageModel.CreateHash(imageData);

			if (data.Images.Contains(imageToUpdate) == false)
				throw new InvalidOperationException("Image does not exist in the collection");
			if (data.Images.Any(image => image != imageToUpdate && image.Hash == hash))
				throw new InvalidOperationException("An image with the same hash already exists");

			collectionProvider.WriteImageFile(storagePath, imageToUpdate, imageData);
			imageToUpdate.Hash = hash;
		}

		public void RemoveImage(ImageModel image)
		{
			bool removed = data.Images.Remove(image);
			if (removed)
				removedImages.Add(image);
		}

		public void Save()
		{
			collectionProvider.SaveCollection(storagePath, data, removedImages);
			removedImages.Clear();
		}

		public bool IsSaved()
		{
			return collectionProvider.IsCollectionSaved(storagePath, data, removedImages);
		}

		public void Export(string destinationPath, IEnumerable<ImageModel> imagesToExport)
		{
			CollectionData exportData = new CollectionData();
			exportData.Images = new List<ImageModel>(imagesToExport);
			collectionProvider.ExportCollection(storagePath, destinationPath, exportData);
		}
	}
}
