using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using JsonSerializerProxy = Overmind.ImageManager.Model.Serialization.JsonSerializer;

namespace Overmind.ImageManager.Test
{
	[TestClass]
	public class CollectionProviderTest
	{
		public TestContext TestContext { get; set; }

		private HashAlgorithm hashAlgorithm;
		private IImageOperations imageOperations;
		private CollectionProvider collectionProvider;
		private string workingDirectory;

		[TestInitialize]
		public void Initialize()
		{
			JsonSerializer serializerImplementation = new JsonSerializer() { Formatting = Formatting.Indented };
			JsonSerializerProxy serializer = new JsonSerializerProxy(serializerImplementation);
			FileNameFormatter fileNameFormatter = new FileNameFormatter();

			hashAlgorithm = MD5.Create();
			imageOperations = new ImageOperations(hashAlgorithm);
			collectionProvider = new CollectionProvider(serializer, imageOperations, fileNameFormatter);
			workingDirectory = Path.Combine(TestContext.TestRunDirectory, "Working", TestContext.TestName);
		}

		[TestCleanup]
		public void Cleanup()
		{
			hashAlgorithm.Dispose();
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_Empty()
		{
			collectionProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_Add()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = imageOperations.ComputeHash(initialImageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);

			Assert.IsNotNull(imageModel.FileNameAsTemporary);
			Assert.IsNull(imageModel.FileNameAsSaved);
			Assert.IsNotNull(imageModel.FileName);

			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsNotNull(imageModel.FileName);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_Rename()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = imageOperations.ComputeHash(initialImageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			string initialFileName = imageModel.FileName;
			imageModel.Title = "Renamed";
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.AreNotEqual(initialFileName, imageModel.FileName);
			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", initialFileName)));
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_Replace()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");
			byte[] updatedImageData = File.ReadAllBytes("Resources/Green.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = imageOperations.ComputeHash(initialImageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			string initialFileName = imageModel.FileNameAsSaved;

			imageModel.Hash = imageOperations.ComputeHash(updatedImageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, updatedImageData);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", initialFileName)));
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_RemoveSaved()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = imageOperations.ComputeHash(imageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));

			collectionData.Images.Remove(imageModel);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>() { imageModel });

			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_RemoveTemporary()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = imageOperations.ComputeHash(imageData);
			collectionProvider.WriteImageFile(workingDirectory, imageModel, imageData);

			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));

			collectionData.Images.Remove(imageModel);
			collectionProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>() { imageModel });

			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
		}

		[TestMethod]
		public void CollectionProvider_SaveCollection_LeftoverFiles()
		{
			collectionProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());

			Directory.CreateDirectory(Path.Combine(workingDirectory, "Data"));
			Directory.CreateDirectory(Path.Combine(workingDirectory, "Data-Temporary"));
			Directory.CreateDirectory(Path.Combine(workingDirectory, "Data-ToRemove"));
			Directory.CreateDirectory(Path.Combine(workingDirectory, "Images"));
			Directory.CreateDirectory(Path.Combine(workingDirectory, "Images-Temporary"));

			File.Create(Path.Combine(workingDirectory, "Data", "Empty.txt")).Close();
			File.Create(Path.Combine(workingDirectory, "Data-Temporary", "Empty.txt")).Close();
			File.Create(Path.Combine(workingDirectory, "Data-ToRemove", "Empty.txt")).Close();
			File.Create(Path.Combine(workingDirectory, "Images", "Empty.txt")).Close();
			File.Create(Path.Combine(workingDirectory, "Images-Temporary", "Empty.txt")).Close();

			collectionProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());
		}

		[TestMethod]
		public void CollectionProvider_WriteImageFile_Jpeg()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.jpg");
			collectionProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
			Assert.IsTrue(imageModel.FileNameAsTemporary.EndsWith(".jpeg"));
		}

		[TestMethod]
		public void CollectionProvider_WriteImageFile_Png()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");
			collectionProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
			Assert.IsTrue(imageModel.FileNameAsTemporary.EndsWith(".png"));
		}

		[TestMethod]
		public void CollectionProvider_WriteImageFile_Text()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = Encoding.UTF8.GetBytes("Not a image");
			Assert.ThrowsException<ArgumentException>(() => collectionProvider.WriteImageFile(workingDirectory, imageModel, imageData));
		}
	}
}
