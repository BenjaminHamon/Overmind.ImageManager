using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionModel : ReadOnlyCollectionModel
	{
		public CollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
			: base(dataProvider, data, storagePath)
		{ }

		private readonly List<ImageModel> removedImages = new List<ImageModel>();

		public void AddImage(ImageModel newImage)
		{
			ImageModel existingImage = data.Images.FirstOrDefault(image => image.Hash == newImage.Hash);
			if (existingImage != null)
				throw new InvalidOperationException("An image with the same hash already exists");

			data.Images.Add(newImage);
		}

		public void RemoveImage(ImageModel image)
		{
			bool removed = data.Images.Remove(image);
			if (removed)
				removedImages.Add(image);
		}

		public void WriteImageFile(ImageModel image, byte[] imageData)
		{
			dataProvider.WriteImageFile(storagePath, image, imageData);
		}

		public void Save()
		{
			dataProvider.SaveCollection(storagePath, data, removedImages);
			removedImages.Clear();
		}

		public bool IsSaved()
		{
			return dataProvider.IsCollectionSaved(storagePath, data, removedImages);
		}

		public void Export(string destinationPath, IEnumerable<ImageModel> imagesToExport)
		{
			CollectionData exportData = new CollectionData();
			exportData.Images = new List<ImageModel>(imagesToExport);
			dataProvider.ExportCollection(storagePath, destinationPath, exportData);
		}
	}
}
