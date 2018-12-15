using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.WindowsClient
{
	public class QueryViewModel : INotifyPropertyChanged
	{
		private const string FieldSeparator = ",";
		private static readonly Regex FieldRegex = new Regex("^(?<name>[a-zA-Z]+)(\\[(?<index>[0-9]+)\\])?$");

		public QueryViewModel(IQueryEngine<ImageModel> queryEngine)
		{
			this.queryEngine = queryEngine;
		}

		private readonly IQueryEngine<ImageModel> queryEngine;

		public event PropertyChangedEventHandler PropertyChanged;

		private string searchExpressionField;
		public string SearchExpression
		{
			get { return searchExpressionField; }
			set
			{
				if (searchExpressionField == value)
					return;
				searchExpressionField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchExpression)));
				Error = null;
			}
		}

		private string groupByExpressionField;
		public string GroupByExpression
		{
			get { return groupByExpressionField; }
			set
			{
				if (groupByExpressionField == value)
					return;
				groupByExpressionField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupByExpression)));
				Error = null;
			}
		}

		private string orderByExpressionField;
		public string OrderByExpression
		{
			get { return orderByExpressionField; }
			set
			{
				if (orderByExpressionField == value)
					return;
				orderByExpressionField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderByExpression)));
				Error = null;
			}
		}

		private string errorField;
		public string Error
		{
			get { return errorField; }
			set
			{
				if (errorField == value)
					return;
				errorField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
			}
		}

		public ICollection<ImageViewModel> Execute(IEnumerable<ImageViewModel> allImages)
		{
			Func<ImageModel, string> groupQuery = CreateGroupQuery(GroupByExpression);
			foreach (ImageViewModel image in allImages)
				image.Group = groupQuery(image.Model);

			ICollection<ImageModel> searchResult = queryEngine.Search(allImages.Select(image => image.Model), SearchExpression);
			IEnumerable<ImageViewModel> resultImages = allImages.Where(image => searchResult.Contains(image.Model));

			resultImages = OrderBy(resultImages, OrderByExpression);

			return new List<ImageViewModel>(resultImages);
		}

		private static Func<ImageModel, string> CreateGroupQuery(string expression)
		{
			if (String.IsNullOrWhiteSpace(expression))
				return image => "#All";

			List<Func<ImageModel, object>> getterList = expression.Split(new string[] { FieldSeparator }, StringSplitOptions.None)
				.Select(fieldExpression => CreateFieldGetter(fieldExpression.Trim())).ToList();
			return image =>
			{
				string value = String.Join(FieldSeparator, getterList.Select(getter => getter(image)));
				if (String.IsNullOrEmpty(value))
					value = "#NoGroup";
				return value;
			};
		}

		private static IEnumerable<ImageViewModel> OrderBy(IEnumerable<ImageViewModel> source, string expression)
		{
			IOrderedEnumerable<ImageViewModel> orderedSource = source.OrderBy(image => image.Group);
			if (String.IsNullOrWhiteSpace(expression))
				return orderedSource;

			List<string> fieldExpressionList = expression.Split(new string[] { FieldSeparator }, StringSplitOptions.None)
				.Select(fieldExpression => fieldExpression.Trim()).ToList();

			foreach (string fieldExpression in fieldExpressionList)
			{
				List<string> fieldExpressionParts = fieldExpression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
				if (fieldExpressionParts.Count > 2)
					throw new Exception(String.Format("Invalid order by field expression '{0}'", fieldExpression));

				Func<ImageModel, object> fieldGetter = CreateFieldGetter(fieldExpressionParts[0]);
				if (fieldExpressionParts.Count == 1)
					fieldExpressionParts.Add("asc");
				switch (fieldExpressionParts[1])
				{
					case "asc": orderedSource = orderedSource.ThenBy(image => fieldGetter(image.Model)); break;
					case "desc": orderedSource = orderedSource.ThenByDescending(image => fieldGetter(image.Model)); break;
					default: throw new Exception(String.Format("Invalid order by field expression '{0}'", fieldExpression));
				}
			}

			return orderedSource;
		}

		private static Func<ImageModel, object> CreateFieldGetter(string fieldExpression)
		{
			Match fieldMatch = FieldRegex.Match(fieldExpression);
			if (fieldMatch.Success == false)
				throw new Exception(String.Format("Invalid field expression '{0}'", fieldExpression));

			string fieldName = fieldMatch.Groups["name"].Value;

			if (fieldMatch.Groups["index"].Success)
			{
				int fieldIndex = Int32.Parse(fieldMatch.Groups["index"].Value);

				switch (fieldName)
				{
					case "subjects": return image => image.SubjectCollection.ElementAtOrDefault(fieldIndex);
					case "artists": return image => image.ArtistCollection.ElementAtOrDefault(fieldIndex);
					default: throw new Exception(String.Format("Unknown field '{0}'", fieldName));
				}
			}
			else
			{
				switch (fieldName)
				{
					case "subjects": return image => String.Join(", ", image.SubjectCollection);
					case "artists": return image => String.Join(", ", image.ArtistCollection);
					case "score": return image => image.Score;
					case "date": return image => image.AdditionDate;
					default: throw new Exception(String.Format("Unknown field '{0}'", fieldName));
				}
			}
		}
	}
}
