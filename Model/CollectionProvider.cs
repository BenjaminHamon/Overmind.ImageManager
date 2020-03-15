using NLog;
using Overmind.ImageManager.Model.Compatibility;
using Overmind.ImageManager.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Overmind.ImageManager.Model
{
	public class CollectionProvider : ICollectionProvider
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(CollectionProvider));

		private const string FormatVersion = "3.0";

		public CollectionProvider(ISerializer serializer, IImageOperations imageOperations, FileNameFormatter fileNameFormatter)
		{
			this.serializer = serializer;
			this.imageOperations = imageOperations;
			this.fileNameFormatter = fileNameFormatter;
		}

		private readonly ISerializer serializer;
		private readonly IImageOperations imageOperations;
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

			CollectionMetadata collectionMetadata = serializer.DeserializeFromFile<CollectionMetadata>(Path.Combine(collectionPath, "Data", "Metadata.json"));
			ISerializer localSerializer = GetSerializerForLoad(collectionMetadata.FormatVersion);

			CollectionData collectionData = new CollectionData() { Metadata = collectionMetadata };

			collectionData.Images = localSerializer.DeserializeFromFile<List<ImageModel>>(Path.Combine(collectionPath, "Data", "Images.json"));
			foreach (ImageModel image in collectionData.Images)
				image.FileNameAsSaved = image.FileName;

			collectionData.Metadata.FormatVersion = FormatVersion;

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

			foreach (ImageModel image in removedImages.Where(i => i.FileNameAsSaved != null))
				File.Delete(Path.Combine(collectionPath, "Images", image.FileNameAsSaved));

			foreach (ImageModel image in collectionData.Images)
			{
				image.FileName = fileNameFormatter.Format(image);

				if (image.FileNameAsTemporary != null)
				{
					if (image.FileNameAsSaved != null)
						File.Delete(Path.Combine(collectionPath, "Images", image.FileNameAsSaved));

					string temporaryPath = Path.Combine(collectionPath, "Images-Temporary", image.FileNameAsTemporary);
					string finalPath = Path.Combine(collectionPath, "Images", image.FileName);

					File.Move(temporaryPath, finalPath);
					image.FileNameAsSaved = image.FileName;
					image.FileNameAsTemporary = null;
				}
				else if (image.FileNameAsSaved != null)
				{
					if (image.FileNameAsSaved != image.FileName)
					{
						string oldPath = Path.Combine(collectionPath, "Images", image.FileNameAsSaved);
						string finalPath = Path.Combine(collectionPath, "Images", image.FileName);

						File.Move(oldPath, finalPath);
						image.FileNameAsSaved = image.FileName;
					}
				}
			}

			Directory.Delete(Path.Combine(collectionPath, "Images-Temporary"), true);

			serializer.SerializeToFile(Path.Combine(collectionPath, "Data-Temporary", "Metadata.json"), collectionData.Metadata);
			serializer.SerializeToFile(Path.Combine(collectionPath, "Data-Temporary", "Images.json"), collectionData.Images);

			if (Directory.Exists(Path.Combine(collectionPath, "Data-ToRemove")))
				Directory.Delete(Path.Combine(collectionPath, "Data-ToRemove"), true);

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

			string activeSerialized = serializer.SerializeToString(activeCollectionData);
			string savedSerialized = serializer.SerializeToString(savedCollectionData);

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
				string destinationImagePath = Path.Combine(destinationCollectionPath, "Images", image.FileNameAsSaved);
				File.Copy(sourceImagePath, destinationImagePath);
			}

			SaveCollection(destinationCollectionPath, collectionData, new List<ImageModel>());
		}

		public string GetImagePath(string collectionPath, ImageModel image)
		{
			if (image.FileNameAsTemporary != null)
				return Path.Combine(collectionPath, "Images-Temporary", image.FileNameAsTemporary);
			if (image.FileNameAsSaved != null)
				return Path.Combine(collectionPath, "Images", image.FileNameAsSaved);
			return null;
		}

		public void WriteImageFile(string collectionPath, ImageModel image, byte[] imageData)
		{
			string fileExtension = imageOperations.GetFormat(imageData);

			Directory.CreateDirectory(Path.Combine(collectionPath, "Images-Temporary"));

			image.FileName = fileNameFormatter.Format(image, fileExtension);
			string temporaryPath = Path.Combine(collectionPath, "Images-Temporary", image.FileName);
			File.WriteAllBytes(temporaryPath, imageData);
			image.FileNameAsTemporary = image.FileName;
		}

		public void ClearUnsavedFiles(string collectionPath)
		{
			string temporaryDirectory = Path.Combine(collectionPath, "Images-Temporary");
			if (Directory.Exists(temporaryDirectory))
				Directory.Delete(temporaryDirectory, true);
		}

		private ISerializer GetSerializerForLoad(string sourceFormatVersion)
		{
			if (sourceFormatVersion == FormatVersion)
				return serializer;

			if (sourceFormatVersion == "2.0")
			{
				Newtonsoft.Json.JsonSerializer compatibilitySerializer = new Newtonsoft.Json.JsonSerializer();
				compatibilitySerializer.Converters.Add(new ImageModel_V2_Converter());
				return new JsonSerializer(compatibilitySerializer);
			}

			throw new InvalidDataException("The collection format version is not supported.");
		}
	}
}
