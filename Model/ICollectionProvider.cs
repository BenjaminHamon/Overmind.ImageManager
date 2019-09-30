using System.Collections.Generic;

namespace Overmind.ImageManager.Model
{
	public interface ICollectionProvider
	{
		CollectionData CreateCollection(string collectionPath);
		CollectionData LoadCollection(string collectionPath);
		void SaveCollection(string collectionPath, CollectionData collectionData, IEnumerable<ImageModel> removedImages);
		bool IsCollectionSaved(string collectionPath, CollectionData activeCollectionData, IEnumerable<ImageModel> removedImages);
		void ExportCollection(string sourceCollectionPath, string destinationCollectionPath, CollectionData collectionData);
		string GetImagePath(string collectionPath, ImageModel image);
		void WriteImageFile(string collectionPath, ImageModel image, byte[] imageData);
		void ClearUnsavedFiles(string collectionPath);
	}
}
