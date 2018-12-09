using Lucene.Net.QueryParsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model.Wallpapers
{
	// Validation is done by a dedicated method rather than through the property setters,
	// to ensure the deserialization succeeds even if the validation logic changes or an external reference is broken.
	// For example, the configuration should load even if the collection has been moved.
	// The application should try to ensure the configuration is valid when saving, and require it before using it.

	[DataContract]
	public class WallpaperConfiguration
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string CollectionPath { get; set; }
		[DataMember]
		public string ImageQuery { get; set; }
		[DataMember]
		public TimeSpan CyclePeriod { get; set; }

		public Dictionary<string, List<Exception>> Validate()
		{
			Dictionary<string, List<Exception>> errorCollection = new Dictionary<string, List<Exception>>()
			{
				{ nameof(Name), new List<Exception>() },
				{ nameof(CollectionPath), new List<Exception>() },
				{ nameof(ImageQuery), new List<Exception>() },
				{ nameof(CyclePeriod), new List<Exception>() },
			};

			if (String.IsNullOrWhiteSpace(Name))
				errorCollection[nameof(Name)].Add(new ArgumentException("The configuration name cannot be empty.", nameof(Name)));

			if (Directory.Exists(CollectionPath) == false)
				errorCollection[nameof(CollectionPath)].Add(new ArgumentException("The collection path does not exist.", nameof(CollectionPath)));

			try
			{
				ReadOnlyCollectionModel.AssertQuery(ImageQuery);
			}
			catch (ParseException exception)
			{
				errorCollection[nameof(ImageQuery)].Add(new ArgumentException("The image query is invalid.", nameof(ImageQuery), exception));
			}

			if (CyclePeriod < TimeSpan.FromSeconds(1))
				errorCollection[nameof(CyclePeriod)].Add(new ArgumentException("The cycle period cannot be smaller than one second.", nameof(CyclePeriod)));

			return errorCollection;
		}
	}
}
