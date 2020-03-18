using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model.Downloads
{
	[DataContract]
	public class DownloadSourceConfiguration
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string DomainName { get; set; }
		[DataMember]
		public string RootResolver { get; set; }
		[DataMember]
		public string UriResolver { get; set; }
		[DataMember]
		public string TitleResolver { get; set; }

		public Dictionary<string, List<Exception>> Validate()
		{
			Dictionary<string, List<Exception>> errorCollection = new Dictionary<string, List<Exception>>()
			{
				{ nameof(Name), new List<Exception>() },
				{ nameof(DomainName), new List<Exception>() },
				{ nameof(RootResolver), new List<Exception>() },
				{ nameof(UriResolver), new List<Exception>() },
				{ nameof(TitleResolver), new List<Exception>() },
			};

			if (String.IsNullOrWhiteSpace(Name))
				errorCollection[nameof(Name)].Add(new ArgumentException("The configuration name cannot be empty.", nameof(Name)));

			if (String.IsNullOrWhiteSpace(DomainName))
				errorCollection[nameof(DomainName)].Add(new ArgumentException("The domain name cannot be empty.", nameof(DomainName)));

			try { Uri baseUri = new Uri("http://" + DomainName + "/"); }
			catch (UriFormatException exception) { errorCollection[nameof(DomainName)].Add(exception); }
			
			return errorCollection;
		}
	}
}
