using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Overmind.ImageManager.Test
{
	[TestClass]
	public class DataProviderTest
	{
		public TestContext TestContext { get; set; }

		private DataProvider dataProvider;
		private string workingDirectory;

		[TestInitialize]
		public void Initialize()
		{
			JsonSerializer serializer = new JsonSerializer();
			FileNameFormatter fileNameFormatter = new FileNameFormatter();

			dataProvider = new DataProvider(serializer, fileNameFormatter);
			workingDirectory = Path.Combine(TestContext.TestRunDirectory, "Working", TestContext.TestName);
		}

		[TestMethod]
		public void DataProvider_SaveCollection_Empty()
		{
			dataProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());
		}

		[TestMethod]
		public void DataProvider_SaveCollection_Add()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = ImageModel.CreateHash(initialImageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);

			Assert.IsNotNull(imageModel.FileNameAsTemporary);
			Assert.IsNull(imageModel.FileNameAsSaved);
			Assert.IsNotNull(imageModel.FileName);

			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsNotNull(imageModel.FileName);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
		}

		[TestMethod]
		public void DataProvider_SaveCollection_Rename()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = ImageModel.CreateHash(initialImageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			string initialFileName = imageModel.FileName;
			imageModel.Title = "Renamed";
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.AreNotEqual(initialFileName, imageModel.FileName);
			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", initialFileName)));
		}

		[TestMethod]
		public void DataProvider_SaveCollection_Replace()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] initialImageData = File.ReadAllBytes("Resources/Red.png");
			byte[] updatedImageData = File.ReadAllBytes("Resources/Green.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = ImageModel.CreateHash(initialImageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, initialImageData);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			string initialFileName = imageModel.FileNameAsSaved;

			imageModel.Hash = ImageModel.CreateHash(updatedImageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, updatedImageData);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());

			Assert.IsNull(imageModel.FileNameAsTemporary);
			Assert.IsNotNull(imageModel.FileNameAsSaved);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", initialFileName)));
		}

		[TestMethod]
		public void DataProvider_SaveCollection_RemoveSaved()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = ImageModel.CreateHash(imageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>());
			
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));

			collectionData.Images.Remove(imageModel);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>() { imageModel });

			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images", imageModel.FileNameAsSaved)));
		}

		[TestMethod]
		public void DataProvider_SaveCollection_RemoveTemporary()
		{
			CollectionData collectionData = new CollectionData();
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");

			collectionData.Images.Add(imageModel);

			imageModel.Hash = ImageModel.CreateHash(imageData);
			dataProvider.WriteImageFile(workingDirectory, imageModel, imageData);

			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));

			collectionData.Images.Remove(imageModel);
			dataProvider.SaveCollection(workingDirectory, collectionData, new List<ImageModel>() { imageModel });

			Assert.IsFalse(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
		}

		[TestMethod]
		public void DataProvider_SaveCollection_LeftoverFiles()
		{
			dataProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());

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

			dataProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());
		}

		[TestMethod]
		public void DataProvider_WriteImageFile_Jpeg()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.jpg");
			dataProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
			Assert.IsTrue(imageModel.FileNameAsTemporary.EndsWith(".jpeg"));
		}

		[TestMethod]
		public void DataProvider_WriteImageFile_Png()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = File.ReadAllBytes("Resources/Red.png");
			dataProvider.WriteImageFile(workingDirectory, imageModel, imageData);
			Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "Images-Temporary", imageModel.FileNameAsTemporary)));
			Assert.IsTrue(imageModel.FileNameAsTemporary.EndsWith(".png"));
		}

		[TestMethod]
		public void DataProvider_WriteImageFile_Text()
		{
			ImageModel imageModel = new ImageModel();
			byte[] imageData = Encoding.UTF8.GetBytes("Not a image");
			Assert.ThrowsException<ArgumentException>(() => dataProvider.WriteImageFile(workingDirectory, imageModel, imageData));
		}
	}
}
