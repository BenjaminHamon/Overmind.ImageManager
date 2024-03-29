﻿using System.Collections.Generic;

namespace Overmind.ImageManager.Model
{
	public class ReadOnlyCollectionModel
	{
		public ReadOnlyCollectionModel(ICollectionProvider collectionProvider, CollectionData data, string storagePath)
		{
			this.collectionProvider = collectionProvider;
			this.data = data;
			this.storagePath = storagePath;
		}

		protected readonly ICollectionProvider collectionProvider;
		protected readonly CollectionData data;
		protected readonly string storagePath;

		public string StoragePath { get { return storagePath; } }
		public IReadOnlyCollection<ImageModel> AllImages { get { return data.Images; } }

		public string GetImagePath(ImageModel image)
		{
			return collectionProvider.GetImagePath(storagePath, image);
		}
	}
}
