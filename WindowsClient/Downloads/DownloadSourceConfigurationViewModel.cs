using Overmind.ImageManager.Model.Downloads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public class DownloadSourceConfigurationViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		public DownloadSourceConfigurationViewModel(DownloadSourceConfiguration model)
		{
			this.Model = model;

			nameField = model.Name;

			ErrorCollection = new Dictionary<string, List<Exception>>();
			WarningCollection = model.Validate();
		}

		public DownloadSourceConfiguration Model { get; }

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
			WarningCollection = Model.Validate();

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
					ErrorCollection[nameof(Name)].Add(new ArgumentException("The configuration name cannot be empty.", nameof(Name)));

				if (ErrorCollection[nameof(Name)].Any() == false)
					Model.Name = nameField;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Name)));

				UpdateValidation();
			}
		}

		public string DomainName
		{
			get { return Model.DomainName; }
			set
			{
				if (Model.DomainName == value)
					return;

				Model.DomainName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DomainName)));

				UpdateValidation();
			}
		}

		public string RootResolver
		{
			get { return Model.RootResolver; }
			set
			{
				if (Model.RootResolver == value)
					return;

				Model.RootResolver = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RootResolver)));

				UpdateValidation();
			}
		}

		public string UriResolver
		{
			get { return Model.UriResolver; }
			set
			{
				if (Model.UriResolver == value)
					return;

				Model.UriResolver = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UriResolver)));

				UpdateValidation();
			}
		}

		public string TitleResolver
		{
			get { return Model.TitleResolver; }
			set
			{
				if (Model.TitleResolver == value)
					return;

				Model.TitleResolver = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleResolver)));

				UpdateValidation();
			}
		}
	}
}
