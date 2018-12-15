using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model.Queries
{
	public class SimpleQueryEngine : IQueryEngine<ImageModel>
	{
		public void AssertQuery(string queryString) { }

		public ICollection<ImageModel> Search(IEnumerable<ImageModel> dataSet, string queryString)
		{
			if (String.IsNullOrEmpty(queryString))
				return new List<ImageModel>(dataSet);

			List<Regex> queryRegexes = queryString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(element => new Regex("^" + Regex.Escape(element).Replace("\\*", ".*") + "$")).ToList();
			return dataSet.Where(image => queryRegexes.All(regex => image.GetSearchableValues().Any(value => regex.IsMatch(value)))).ToList();
		}
	}
}
