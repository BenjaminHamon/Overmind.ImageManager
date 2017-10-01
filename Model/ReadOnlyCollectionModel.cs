using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model
{
	public class ReadOnlyCollectionModel
	{
		public ReadOnlyCollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
		{
			this.dataProvider = dataProvider;
			this.data = data;
			this.storagePath = storagePath;
		}

		protected readonly DataProvider dataProvider;
		protected readonly CollectionData data;
		protected readonly string storagePath;

		public string Name { get { return storagePath; } }
		public IEnumerable<ImageModel> Images { get { return data.Images; } }

		public string GetImagePath(ImageModel image)
		{
			return dataProvider.GetImagePath(storagePath, image);
		}

		public Func<ImageModel, bool> CreateSearchQuery(string queryString)
		{
			List<Regex> queryRegexes = queryString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(element => new Regex("^" + Regex.Escape(element).Replace("\\*", ".*") + "$")).ToList();
			return image => queryRegexes.All(regex => image.GetSearchableValues().Any(value => regex.IsMatch(value)));
		}
	}
}
