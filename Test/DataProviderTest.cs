using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Overmind.ImageManager.Model;
using System.Collections.Generic;
using System.IO;

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
			workingDirectory = Path.Combine(TestContext.TestRunDirectory, "Data");
		}

		[TestMethod]
		public void DataProvider_SaveCollection_Empty()
		{
			dataProvider.SaveCollection(workingDirectory, new CollectionData(), new List<ImageModel>());
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
	}
}
