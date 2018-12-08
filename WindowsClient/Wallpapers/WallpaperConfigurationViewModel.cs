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
		public WallpaperConfigurationViewModel(WallpaperConfiguration model)
		{
			this.Model = model;

			nameField = model.Name;
			cyclePeriodField = model.CyclePeriod.ToString();
		}

		public WallpaperConfiguration Model { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		private readonly Dictionary<string, List<Exception>> errorCollection = new Dictionary<string, List<Exception>>();

		public bool HasErrors { get { return errorCollection.SelectMany(kvp => kvp.Value).Any(); } }

		public IEnumerable GetErrors(string propertyName)
		{
			if ((propertyName != null) && errorCollection.ContainsKey(propertyName))
				return errorCollection[propertyName];
			return null;
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
				errorCollection[nameof(Name)] = new List<Exception>();

				if (String.IsNullOrWhiteSpace(nameField))
					errorCollection[nameof(Name)].Add(new ArgumentException("The configuration name cannot be empty.", nameof(Name)));

				if (errorCollection[nameof(Name)].Any() == false)
					Model.Name = nameField;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Name)));
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
				errorCollection[nameof(CyclePeriod)] = new List<Exception>();

				// Fix validation not updating if the error changes
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(CyclePeriod)));

				TimeSpan parsedValue;
				if (TimeSpan.TryParseExact(cyclePeriodField, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out parsedValue) == false)
					errorCollection[nameof(CyclePeriod)].Add(new ArgumentException("The cycle period must use the format 'hh:mm:ss'", nameof(CyclePeriod)));
				else if (parsedValue < TimeSpan.FromSeconds(1))
					errorCollection[nameof(CyclePeriod)].Add(new ArgumentException("The cycle period cannot be smaller than one second", nameof(CyclePeriod)));

				if (errorCollection[nameof(CyclePeriod)].Any() == false)
					Model.CyclePeriod = parsedValue;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CyclePeriod)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(CyclePeriod)));
			}
		}
	}
}
