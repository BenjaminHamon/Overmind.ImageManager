using System.Collections.Generic;

namespace Overmind.ImageManager.Model
{
	public class ReadOnlyCollectionModel
	{
		public ReadOnlyCollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
		{
			this.dataProvider = dataProvider;
			this.data = data;
			this.storagePath = storagePath;
		}

		protected readonly DataProvider dataProvider;
		protected readonly CollectionData data;
		protected readonly string storagePath;

		public string StoragePath { get { return storagePath; } }
		public IReadOnlyCollection<ImageModel> AllImages { get { return data.Images; } }

		public string GetImagePath(ImageModel image)
		{
			return dataProvider.GetImagePath(storagePath, image);
		}
	}
}
