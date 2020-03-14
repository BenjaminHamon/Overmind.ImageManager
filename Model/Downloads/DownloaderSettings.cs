using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model.Downloads
{
	[DataContract]
	public class DownloaderSettings
	{
		[DataMember]
		public List<DownloadSourceConfiguration> SourceConfigurationCollection { get; set; } = new List<DownloadSourceConfiguration>();

		public Dictionary<string, List<Exception>> Validate()
		{
			Dictionary<string, List<Exception>> errorCollection = new Dictionary<string, List<Exception>>()
			{
				{ nameof(SourceConfigurationCollection), new List<Exception>() },
			};

			IEnumerable<string> configurationNameDuplicates = SourceConfigurationCollection
				.GroupBy(configuration => configuration.Name).Where(group => group.Count() > 1).Select(group => group.Key);

			foreach (string nameDuplicate in configurationNameDuplicates)
			{
				Exception exception = new ArgumentException(String.Format("The configuration name '{0}' is used several times.", nameDuplicate));
				errorCollection[nameof(SourceConfigurationCollection)].Add(exception);
			}

			return errorCollection;
		}
	}
}
