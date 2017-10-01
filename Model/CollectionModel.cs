using System;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionModel : ReadOnlyCollectionModel, IDisposable
	{
		public CollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
			: base(dataProvider, data, storagePath)
		{ }

		public void AddImage(ImageModel newImage, byte[] newImageData)
		{
			ImageModel existingImage = data.Images.FirstOrDefault(image => image.Hash == newImage.Hash);
			if (existingImage != null)
				throw new InvalidOperationException("An image with the same hash already exists");

			dataProvider.AddImage(storagePath, newImage, newImageData);
			data.Images.Add(newImage);
		}

		public void Save()
		{
			dataProvider.SaveCollection(storagePath, data);
		}

		public void Dispose()
		{
			dataProvider.CleanTemporary(storagePath);
		}
	}
}
