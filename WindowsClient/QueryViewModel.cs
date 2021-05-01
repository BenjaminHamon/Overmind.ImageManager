using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.WindowsClient
{
	public class QueryViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		private const string FieldSeparator = ",";
		private static readonly Regex FieldRegex = new Regex("^(?<name>[a-zA-Z]+)(\\[(?<index>[0-9]+)\\])?$");

		public QueryViewModel(IQueryEngine<ImageModel> queryEngine, Func<Random> randomFactory)
		{
			this.queryEngine = queryEngine;
			this.randomFactory = randomFactory;

			ErrorCollection = new Dictionary<string, List<Exception>>();
		}

		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly Func<Random> randomFactory;

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public Dictionary<string, List<Exception>> ErrorCollection { get; private set; }

		public bool HasErrors { get { return ErrorCollection.SelectMany(kvp => kvp.Value).Any(); } }

		public IEnumerable GetErrors(string propertyName)
		{
			if ((propertyName != null) && ErrorCollection.ContainsKey(propertyName))
				return ErrorCollection[propertyName];
			return null;
		}

		private string searchExpressionField;
		public string SearchExpression
		{
			get { return searchExpressionField; }
			set
			{
				if (searchExpressionField == value)
					return;

				searchExpressionField = value;
				ErrorCollection[nameof(SearchExpression)] = new List<Exception>();

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchExpression)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SearchExpression)));
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
				ErrorCollection[nameof(GroupByExpression)] = new List<Exception>();

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupByExpression)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(GroupByExpression)));
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
				ErrorCollection[nameof(OrderByExpression)] = new List<Exception>();

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderByExpression)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(OrderByExpression)));
			}
		}

		public void Validate()
		{
			ErrorCollection[nameof(SearchExpression)] = new List<Exception>();
			ErrorCollection[nameof(GroupByExpression)] = new List<Exception>();
			ErrorCollection[nameof(OrderByExpression)] = new List<Exception>();

			try { Filter(new List<ImageViewModel>(), SearchExpression); }
			catch (Exception exception) { ErrorCollection[nameof(SearchExpression)].Add(exception); }

			try { GroupBy(new List<ImageViewModel>(), GroupByExpression); }
			catch (Exception exception) { ErrorCollection[nameof(GroupByExpression)].Add(exception); }

			try { OrderBy(new List<ImageViewModel>(), OrderByExpression); }
			catch (Exception exception) { ErrorCollection[nameof(OrderByExpression)].Add(exception); }

			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SearchExpression)));
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(GroupByExpression)));
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(OrderByExpression)));
		}

		public ICollection<ImageViewModel> Execute(IEnumerable<ImageViewModel> allImages)
		{
			foreach (ImageViewModel image in allImages)
				image.Group = null;

			IEnumerable<ImageViewModel> resultImages = allImages;
			resultImages = Filter(resultImages, SearchExpression);
			resultImages = GroupBy(resultImages, GroupByExpression);
			resultImages = OrderBy(resultImages, OrderByExpression);

			return new List<ImageViewModel>(resultImages);
		}

		private IEnumerable<ImageViewModel> Filter(IEnumerable<ImageViewModel> source, string expression)
		{
			ICollection<ImageModel> searchResult = queryEngine.Search(source.Select(image => image.Model), expression);
			Dictionary<ImageModel, ImageViewModel> sourceAsDictionary = source.ToDictionary(image => image.Model, x => x);
			return searchResult.Select(image => sourceAsDictionary[image]);
		}

		private IEnumerable<ImageViewModel> GroupBy(IEnumerable<ImageViewModel> source, string expression)
		{
			List<Func<ImageModel, object>> getterList = new List<Func<ImageModel, object>>();

			if (String.IsNullOrWhiteSpace(expression))
			{
				getterList.Add(image => "#All");
			}
			else
			{
				foreach (string fieldExpression in expression.Split(new string[] { FieldSeparator }, StringSplitOptions.None))
					getterList.Add(CreateFieldGetter(fieldExpression.Trim()));
			}

			foreach (ImageViewModel image in source)
			{
				string value = String.Join(FieldSeparator, getterList.Select(getter => getter(image.Model)));
				image.Group = String.IsNullOrEmpty(value) ? "#NoGroup" : value;
			}

			return source;
		}

		private IEnumerable<ImageViewModel> OrderBy(IEnumerable<ImageViewModel> source, string expression)
		{
			IOrderedEnumerable<ImageViewModel> orderedSource = source.OrderBy(image => image.Group);
			if (String.IsNullOrWhiteSpace(expression))
				return orderedSource;

			List<string> fieldExpressionList = expression.Split(new string[] { FieldSeparator }, StringSplitOptions.None)
				.Select(fieldExpression => fieldExpression.Trim()).ToList();

			foreach (string fieldExpression in fieldExpressionList)
			{
				if (fieldExpression == "random")
				{
					Random random = randomFactory();
					orderedSource = orderedSource.ThenBy(image => random.Next());
				}
				else
				{
					List<string> fieldExpressionParts = fieldExpression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
					if (fieldExpressionParts.Count > 2)
						throw new ArgumentException(String.Format("Invalid order by field expression '{0}'", fieldExpression));

					Func<ImageModel, object> fieldGetter = CreateFieldGetter(fieldExpressionParts[0]);
					if (fieldExpressionParts.Count == 1)
						fieldExpressionParts.Add("asc");
					switch (fieldExpressionParts[1])
					{
						case "asc": orderedSource = orderedSource.ThenBy(image => fieldGetter(image.Model)); break;
						case "desc": orderedSource = orderedSource.ThenByDescending(image => fieldGetter(image.Model)); break;
						default: throw new ArgumentException(String.Format("Invalid order by field expression '{0}'", fieldExpression));
					}
				}
			}

			return orderedSource;
		}

		private Func<ImageModel, object> CreateFieldGetter(string fieldExpression)
		{
			Match fieldMatch = FieldRegex.Match(fieldExpression);
			if (fieldMatch.Success == false)
				throw new ArgumentException(String.Format("Invalid field expression '{0}'", fieldExpression));

			string fieldName = fieldMatch.Groups["name"].Value;

			if (fieldMatch.Groups["index"].Success)
			{
				int fieldIndex = Int32.Parse(fieldMatch.Groups["index"].Value);

				switch (fieldName)
				{
					case "subjects": return image => image.SubjectCollection.ElementAtOrDefault(fieldIndex);
					case "artists": return image => image.ArtistCollection.ElementAtOrDefault(fieldIndex);
					default: throw new ArgumentException(String.Format("Unknown field '{0}'", fieldName));
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
					default: throw new ArgumentException(String.Format("Unknown field '{0}'", fieldName));
				}
			}
		}
	}
}
