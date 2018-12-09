using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class DataProvider
	{
		public const string ImageCollectionFileName = "images.json";
		private const string TemporaryDirectory = ".temporary";

		public DataProvider(JsonSerializer serializer, FileNameFormatter fileNameFormatter)
		{
			this.serializer = serializer;
			this.fileNameFormatter = fileNameFormatter;
		}

		private readonly JsonSerializer serializer;
		private readonly FileNameFormatter fileNameFormatter;

		public CollectionData CreateCollection(string collectionPath)
		{
			if (Directory.Exists(collectionPath) && Directory.EnumerateFileSystemEntries(collectionPath).Any())
				throw new ArgumentException("Directory is not empty", nameof(collectionPath));

			CollectionData newCollection = new CollectionData();
			SaveCollection(collectionPath, newCollection, new List<ImageModel>());
			return newCollection;
		}

		public CollectionData LoadCollection(string collectionPath)
		{
			string jsonPath = Path.Combine(collectionPath, ImageCollectionFileName);

			CollectionData collectionData = new CollectionData();
			using (StreamReader streamReader = new StreamReader(jsonPath))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				collectionData.Images = serializer.Deserialize<List<ImageModel>>(jsonReader);

			foreach (ImageModel image in collectionData.Images)
				image.FileNameInStorage = image.FileName;

			CleanTemporary(collectionPath);

			return collectionData;
		}

		public void SaveCollection(string collectionPath, CollectionData collectionData, IEnumerable<ImageModel> removedImages)
		{
			Directory.CreateDirectory(collectionPath);
			string jsonPath = Path.Combine(collectionPath, ImageCollectionFileName);

			foreach (ImageModel image in removedImages)
				File.Delete(Path.Combine(collectionPath, image.FileNameInStorage));

			foreach (ImageModel image in collectionData.Images)
			{
				image.FileName = fileNameFormatter.Format(image);

				string temporaryPath = Path.Combine(collectionPath, DataProvider.TemporaryDirectory, image.FileNameInStorage);
				string oldPath = Path.Combine(collectionPath, image.FileNameInStorage);
				string finalPath = Path.Combine(collectionPath, image.FileName);

				if (File.Exists(temporaryPath))
				{
					if (File.Exists(oldPath))
						File.Delete(oldPath);

					File.Move(temporaryPath, finalPath);
					image.FileNameInStorage = image.FileName;
				}
				else if (File.Exists(oldPath))
				{
					if (image.FileNameInStorage != image.FileName)
					{
						File.Move(oldPath, finalPath);
						image.FileNameInStorage = image.FileName;
					}
				}
				else
				{
					System.Diagnostics.Trace.TraceWarning("[DataProvider] File not found: {0}", oldPath);
				}
			}

			CleanTemporary(collectionPath);

			using (StreamWriter streamWriter = new StreamWriter(jsonPath))
			using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
				serializer.Serialize(jsonWriter, collectionData.Images);
		}

		public bool IsCollectionSaved(string collectionPath, CollectionData activeCollectionData, IEnumerable<ImageModel> removedImages)
		{
			if (removedImages.Any())
				return false;

			string temporaryDirectory = Path.Combine(collectionPath, DataProvider.TemporaryDirectory);
			if (Directory.Exists(temporaryDirectory))
				return false;

			CollectionData savedCollectionData = new CollectionData();
			string jsonPath = Path.Combine(collectionPath, ImageCollectionFileName);
			using (StreamReader streamReader = new StreamReader(jsonPath))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				savedCollectionData.Images = serializer.Deserialize<List<ImageModel>>(jsonReader);

			string activeSerialized = SerializeToString(activeCollectionData);
			string savedSerialized = SerializeToString(savedCollectionData);

			return activeSerialized == savedSerialized;
		}

		public void ExportCollection(string sourceCollectionPath, string destinationCollectionPath, CollectionData collectionData)
		{
			if (Directory.Exists(destinationCollectionPath) && Directory.EnumerateFileSystemEntries(destinationCollectionPath).Any())
				throw new ArgumentException("Directory is not empty", nameof(destinationCollectionPath));

			Directory.CreateDirectory(destinationCollectionPath);
			foreach (ImageModel image in collectionData.Images)
				File.Copy(Path.Combine(sourceCollectionPath, image.FileNameInStorage), Path.Combine(destinationCollectionPath, image.FileNameInStorage));

			SaveCollection(destinationCollectionPath, collectionData, new List<ImageModel>());
		}

		public string GetImagePath(string collectionPath, ImageModel image)
		{
			string temporaryPath = Path.Combine(collectionPath, DataProvider.TemporaryDirectory, image.FileNameInStorage);
			if (File.Exists(temporaryPath))
				return temporaryPath;
			return Path.Combine(collectionPath, image.FileName);
		}

		public void WriteImageFile(string collectionPath, ImageModel image, byte[] imageData)
		{
			string fileExtension;

			using (MemoryStream stream = new MemoryStream(imageData))
			using (Image imageObject = Image.FromStream(stream))
				fileExtension = new ImageFormatConverter().ConvertToString(imageObject.RawFormat).ToLower();

			string temporaryDirectory = Path.Combine(collectionPath, DataProvider.TemporaryDirectory);
			Directory.CreateDirectory(temporaryDirectory);

			image.FileName = fileNameFormatter.Format(image, fileExtension);
			string temporaryPath = Path.Combine(temporaryDirectory, image.FileName);
			File.WriteAllBytes(temporaryPath, imageData);
			image.FileNameInStorage = image.FileName;
		}

		public void CleanTemporary(string collectionPath)
		{
			string temporaryDirectory = Path.Combine(collectionPath, DataProvider.TemporaryDirectory);
			if (Directory.Exists(temporaryDirectory))
				Directory.Delete(temporaryDirectory, true);
		}

		private string SerializeToString<TData>(TData data)
		{
			using (StringWriter stringWriter = new StringWriter())
			using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
			{
				serializer.Serialize(jsonWriter, data);
				return stringWriter.ToString();
			}
		}
	}
}
