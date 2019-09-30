using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;

namespace Overmind.ImageManager.Test
{
	internal class FakeCollectionProvider : ICollectionProvider
	{
		internal bool WriteImageFile_ThrowException = false;

		public CollectionData CreateCollection(string collectionPath)
		{
			return null;
		}

		public CollectionData LoadCollection(string collectionPath)
		{
			return null;
		}

		public void SaveCollection(string collectionPath, CollectionData collectionData, IEnumerable<ImageModel> removedImages)
		{

		}

		public bool IsCollectionSaved(string collectionPath, CollectionData activeCollectionData, IEnumerable<ImageModel> removedImages)
		{
			return false;
		}

		public void ExportCollection(string sourceCollectionPath, string destinationCollectionPath, CollectionData collectionData)
		{

		}

		public string GetImagePath(string collectionPath, ImageModel image)
		{
			return null;
		}

		public void WriteImageFile(string collectionPath, ImageModel image, byte[] imageData)
		{
			if (WriteImageFile_ThrowException)
				throw new InvalidOperationException();
		}

		public void ClearUnsavedFiles(string collectionPath)
		{

		}
	}
}
