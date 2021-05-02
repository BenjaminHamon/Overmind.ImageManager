using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.Model.Wallpapers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public class WallpaperConfigurationViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		public WallpaperConfigurationViewModel(WallpaperConfiguration model, IQueryEngine<ImageModel> queryEngine)
		{
			this.Model = model;
			this.queryEngine = queryEngine;

			nameField = model.Name;
			cyclePeriodField = model.CyclePeriod.ToString();

			ErrorCollection = new Dictionary<string, List<Exception>>();
			WarningCollection = model.Validate(queryEngine);
		}

		public WallpaperConfiguration Model { get; }

		private readonly IQueryEngine<ImageModel> queryEngine;

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public Dictionary<string, List<Exception>> ErrorCollection { get; private set; }
		public Dictionary<string, List<Exception>> WarningCollection { get; private set; }

		public bool HasErrors { get { return ErrorCollection.SelectMany(kvp => kvp.Value).Any(); } }
		public bool HasWarnings { get { return WarningCollection.SelectMany(kvp => kvp.Value).Any(); } }

		public IEnumerable GetErrors(string propertyName)
		{
			if ((propertyName != null) && ErrorCollection.ContainsKey(propertyName))
				return ErrorCollection[propertyName];
			return null;
		}

		private void UpdateValidation()
		{
			WarningCollection = Model.Validate(queryEngine);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasWarnings)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WarningCollection)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorCollection)));
		}

		public string Title { get { return Model.Name; } }

		private string nameField;
		public string Name
		{
			get { return nameField; }
			set
			{
				if (nameField == value)
					return;

				nameField = value.Trim();
				ErrorCollection[nameof(Name)] = new List<Exception>();

				if (String.IsNullOrWhiteSpace(nameField))
				{
					ErrorCollection[nameof(Name)].Add(new ArgumentException("The configuration name cannot be empty.", nameof(Name)));
				}

				if (ErrorCollection[nameof(Name)].Any() == false)
				{
					Model.Name = nameField;
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Name)));

				UpdateValidation();
			}
		}

		public string CollectionPath
		{
			get { return Model.CollectionPath; }
			set
			{
				if (Model.CollectionPath == value)
					return;

				Model.CollectionPath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollectionPath)));

				UpdateValidation();
			}
		}

		public string ImageQuery
		{
			get { return Model.ImageQuery; }
			set
			{
				if (Model.ImageQuery == value)
					return;

				Model.ImageQuery = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageQuery)));

				UpdateValidation();
			}
		}

		private string cyclePeriodField;
		public string CyclePeriod
		{
			get { return cyclePeriodField; }
			set
			{
				if (cyclePeriodField == value)
					return;

				cyclePeriodField = value;
				ErrorCollection[nameof(CyclePeriod)] = new List<Exception>();

				TimeSpan parsedValue;
				if (TimeSpan.TryParseExact(cyclePeriodField, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out parsedValue) == false)
				{
					ErrorCollection[nameof(CyclePeriod)].Add(new ArgumentException("The cycle period must use the format 'hh:mm:ss'.", nameof(CyclePeriod)));
				}

				if (ErrorCollection[nameof(CyclePeriod)].Any() == false)
				{
					Model.CyclePeriod = parsedValue;
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CyclePeriod)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(CyclePeriod)));

				UpdateValidation();
			}
		}
	}
}
