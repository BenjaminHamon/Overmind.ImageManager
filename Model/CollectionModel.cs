using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionModel : IDisposable
	{
		public CollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
		{
			this.dataProvider = dataProvider;
			this.data = data;
			this.StoragePath = storagePath;
		}

		private readonly DataProvider dataProvider;
		private readonly CollectionData data;

		public string Name { get { return StoragePath; } }
		public string StoragePath { get; }
		public IEnumerable<ImageModel> Images { get { return data.Images; } }

		public void AddImage(ImageModel newImage, byte[] newImageData)
		{
			ImageModel existingImage = data.Images.FirstOrDefault(image => image.Hash == newImage.Hash);
			if (existingImage != null)
				throw new InvalidOperationException("An image with the same hash already exists");

			dataProvider.AddImage(StoragePath, newImage, newImageData);
			data.Images.Add(newImage);
		}

		public void Save()
		{
			dataProvider.SaveCollection(StoragePath, data);
		}

		public void Dispose()
		{
			dataProvider.CleanTemporary(StoragePath);
		}
	}
}
