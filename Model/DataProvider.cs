using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class DataProvider
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(DataProvider));

		private const string FormatVersion = "2.0";

		public DataProvider(JsonSerializer serializer, FileNameFormatter fileNameFormatter)
		{
			this.serializer = serializer;
			this.fileNameFormatter = fileNameFormatter;
		}

		private readonly JsonSerializer serializer;
		private readonly FileNameFormatter fileNameFormatter;

		public CollectionData CreateCollection(string collectionPath)
		{
			Logger.Info("Creating collection (Path: '{0}')", collectionPath);

			if (Directory.Exists(collectionPath) && Directory.EnumerateFileSystemEntries(collectionPath).Any())
				throw new ArgumentException("The directory is not empty.", nameof(collectionPath));

			CollectionData newCollection = new CollectionData();
			SaveCollection(collectionPath, newCollection, new List<ImageModel>());
			return newCollection;
		}

		public CollectionData LoadCollection(string collectionPath)
		{
			Logger.Info("Loading collection (Path: '{0}')", collectionPath);

			CollectionData collectionData = new CollectionData();

			collectionData.Metadata = LoadData<CollectionMetadata>(Path.Combine(collectionPath, "Data", "Metadata.json"));
			if (collectionData.Metadata.FormatVersion != FormatVersion)
				throw new InvalidDataException("The collection format version is not supported.");

			collectionData.Images = LoadData<List<ImageModel>>(Path.Combine(collectionPath, "Data", "Images.json"));
			foreach (ImageModel image in collectionData.Images)
				image.FileNameInStorage = image.FileName;

			return collectionData;
		}

		public void SaveCollection(string collectionPath, CollectionData collectionData, IEnumerable<ImageModel> removedImages)
		{
			Logger.Info("Saving collection (Path: '{0}')", collectionPath);

			collectionData.Metadata.FormatVersion = FormatVersion;

			Directory.CreateDirectory(Path.Combine(collectionPath, "Data"));
			Directory.CreateDirectory(Path.Combine(collectionPath, "Data-Temporary"));
			Directory.CreateDirectory(Path.Combine(collectionPath, "Images"));
			Directory.CreateDirectory(Path.Combine(collectionPath, "Images-Temporary"));

			foreach (ImageModel image in removedImages)
				File.Delete(Path.Combine(collectionPath, "Images", image.FileNameInStorage));

			foreach (ImageModel image in collectionData.Images)
			{
				image.FileName = fileNameFormatter.Format(image);

				string temporaryPath = Path.Combine(collectionPath, "Images-Temporary", image.FileNameInStorage);
				string oldPath = Path.Combine(collectionPath, "Images", image.FileNameInStorage);
				string finalPath = Path.Combine(collectionPath, "Images", image.FileName);

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
					Logger.Warn("Image file not found (Path: '{0}')", oldPath);
				}
			}

			Directory.Delete(Path.Combine(collectionPath, "Images-Temporary"), true);

			SaveData(Path.Combine(collectionPath, "Data-Temporary", "Metadata.json"), collectionData.Metadata);
			SaveData(Path.Combine(collectionPath, "Data-Temporary", "Images.json"), collectionData.Images);

			Directory.Move(Path.Combine(collectionPath, "Data"), Path.Combine(collectionPath, "Data-ToRemove"));
			Directory.Move(Path.Combine(collectionPath, "Data-Temporary"), Path.Combine(collectionPath, "Data"));
			Directory.Delete(Path.Combine(collectionPath, "Data-ToRemove"), true);
		}

		public bool IsCollectionSaved(string collectionPath, CollectionData activeCollectionData, IEnumerable<ImageModel> removedImages)
		{
			if (removedImages.Any())
				return false;

			if (Directory.Exists(Path.Combine(collectionPath, "Images-Temporary")))
				return false;

			CollectionData savedCollectionData = LoadCollection(collectionPath);

			string activeSerialized = SerializeToString(activeCollectionData);
			string savedSerialized = SerializeToString(savedCollectionData);

			return activeSerialized == savedSerialized;
		}

		public void ExportCollection(string sourceCollectionPath, string destinationCollectionPath, CollectionData collectionData)
		{
			Logger.Info("Exporting collection ('{0}' => '{1}')", sourceCollectionPath, destinationCollectionPath);

			if (Directory.Exists(destinationCollectionPath) && Directory.EnumerateFileSystemEntries(destinationCollectionPath).Any())
				throw new ArgumentException("The directory is not empty.", nameof(destinationCollectionPath));

			Directory.CreateDirectory(Path.Combine(destinationCollectionPath, "Images"));

			foreach (ImageModel image in collectionData.Images)
			{
				string sourceImagePath = GetImagePath(sourceCollectionPath, image);
				string destinationImagePath = Path.Combine(destinationCollectionPath, "Images", image.FileNameInStorage);

				if (sourceImagePath != null)
					File.Copy(sourceImagePath, destinationImagePath);
			}

			SaveCollection(destinationCollectionPath, collectionData, new List<ImageModel>());
		}

		public string GetImagePath(string collectionPath, ImageModel image)
		{
			string temporaryPath = Path.Combine(collectionPath, "Images-Temporary", image.FileNameInStorage);
			if (File.Exists(temporaryPath))
				return temporaryPath;

			string finalPath = Path.Combine(collectionPath, "Images", image.FileNameInStorage);
			if (File.Exists(finalPath))
				return finalPath;

			return null;
		}

		public void WriteImageFile(string collectionPath, ImageModel image, byte[] imageData)
		{
			string fileExtension;

			using (MemoryStream stream = new MemoryStream(imageData))
			using (Image imageObject = Image.FromStream(stream))
				fileExtension = new ImageFormatConverter().ConvertToString(imageObject.RawFormat).ToLower();

			Directory.CreateDirectory(Path.Combine(collectionPath, "Images-Temporary"));

			image.FileName = fileNameFormatter.Format(image, fileExtension);
			string temporaryPath = Path.Combine(collectionPath, "Images-Temporary", image.FileName);
			File.WriteAllBytes(temporaryPath, imageData);
			image.FileNameInStorage = image.FileName;
		}

		public void ClearUnsavedFiles(string collectionPath)
		{
			string temporaryDirectory = Path.Combine(collectionPath, "Images-Temporary");
			if (Directory.Exists(temporaryDirectory))
				Directory.Delete(temporaryDirectory, true);
		}

		private TData LoadData<TData>(string filePath)
		{
			using (StreamReader streamReader = new StreamReader(filePath))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize<TData>(jsonReader);
		}

		private void SaveData<TData>(string filePath, TData data)
		{
			using (StreamWriter streamWriter = new StreamWriter(filePath))
			using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
				serializer.Serialize(jsonWriter, data);
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
