using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionModel : ReadOnlyCollectionModel, IDisposable
	{
		public CollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
			: base(dataProvider, data, storagePath)
		{ }

		private readonly List<ImageModel> removedImages = new List<ImageModel>();

		public void AddImage(ImageModel newImage, byte[] newImageData)
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

		public void Dispose()
		{
			dataProvider.CleanTemporary(storagePath);
		}
	}
}
