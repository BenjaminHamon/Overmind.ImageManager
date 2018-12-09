using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model.Wallpapers
{
	[DataContract]
	public class WallpaperSettings
	{
		[DataMember(Name = "Configurations")]
		public List<WallpaperConfiguration> ConfigurationCollection { get; set; } = new List<WallpaperConfiguration>();

		public Dictionary<string, List<Exception>> Validate()
		{
			Dictionary<string, List<Exception>> errorCollection = new Dictionary<string, List<Exception>>()
			{
				{ nameof(ConfigurationCollection), new List<Exception>() },
			};

			IEnumerable<string> configurationNameDuplicates = ConfigurationCollection
				.GroupBy(configuration => configuration.Name).Where(group => group.Count() > 1).Select(group => group.Key);

			foreach (string nameDuplicate in configurationNameDuplicates)
			{
				Exception exception = new ArgumentException(String.Format("The collection name '{0}' is used several times.", nameDuplicate));
				errorCollection[nameof(ConfigurationCollection)].Add(exception);
			}

			return errorCollection;
		}
	}
}
