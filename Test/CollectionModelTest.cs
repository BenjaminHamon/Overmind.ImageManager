using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overmind.ImageManager.Model;
using System;
using System.IO;

namespace Overmind.ImageManager.Test
{
	[TestClass]
	public class CollectionModelTest
	{
		[TestMethod]
		public void CollectionModel_CreateImage_Success()
		{
			ICollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			Assert.AreEqual(0, collection.AllImages.Count);

			ImageModel image = collection.CreateImage(null, imageData);

			Assert.AreEqual(1, collection.AllImages.Count);
			Assert.IsNotNull(image.Hash);
		}

		[TestMethod]
		public void CollectionModel_CreateImage_AlreadyExists()
		{
			ICollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");
			collection.CreateImage(null, imageData);

			Assert.AreEqual(1, collection.AllImages.Count);
			Assert.ThrowsException<InvalidOperationException>(() => collection.CreateImage(null, imageData));
			Assert.AreEqual(1, collection.AllImages.Count);
		}

		[TestMethod]
		public void CollectionModel_CreateImage_ProviderException()
		{
			FakeCollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			collectionProvider.WriteImageFile_ThrowException = true;

			Assert.AreEqual(0, collection.AllImages.Count);
			Assert.ThrowsException<InvalidOperationException>(() => collection.CreateImage(null, imageData));
			Assert.AreEqual(0, collection.AllImages.Count);
		}

		[TestMethod]
		public void CollectionModel_UpdateImageFile_Success()
		{
			ICollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");
			byte[] updatedImageData = File.ReadAllBytes("Resources/Green.png");

			ImageModel image = collection.CreateImage(null, initialImageData);
			collection.UpdateImageFile(image, updatedImageData);
		}

		[TestMethod]
		public void CollectionModel_UpdateImageFile_AlreadyExists()
		{
			ICollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] firstImageData = File.ReadAllBytes("Resources/Red.png");
			byte[] secondImageData = File.ReadAllBytes("Resources/Green.png");

			ImageModel firstImage = collection.CreateImage(null, firstImageData);
			ImageModel secondImage = collection.CreateImage(null, secondImageData);

			Assert.AreNotEqual(firstImage.Hash, secondImage.Hash);
			Assert.ThrowsException<InvalidOperationException>(() => collection.UpdateImageFile(firstImage, secondImageData));
			Assert.AreNotEqual(firstImage.Hash, secondImage.Hash);
		}

		[TestMethod]
		public void CollectionModel_UpdateImageFile_SameImageData()
		{
			ICollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			ImageModel image = collection.CreateImage(null, imageData);
			collection.UpdateImageFile(image, imageData);
		}

		[TestMethod]
		public void CollectionModel_UpdateImageFile_ProviderException()
		{
			FakeCollectionProvider collectionProvider = new FakeCollectionProvider();
			CollectionModel collection = new CollectionModel(collectionProvider, new CollectionData(), null);
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");
			byte[] updatedImageData = File.ReadAllBytes("Resources/Green.png");

			ImageModel image = collection.CreateImage(null, initialImageData);
			string imageHash = image.Hash;

			collectionProvider.WriteImageFile_ThrowException = true;

			Assert.AreEqual(imageHash, image.Hash);
			Assert.ThrowsException<InvalidOperationException>(() => collection.UpdateImageFile(image, updatedImageData));
			Assert.AreEqual(imageHash, image.Hash);
		}
	}
}
