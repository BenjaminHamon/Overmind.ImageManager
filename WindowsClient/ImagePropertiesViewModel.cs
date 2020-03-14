using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImagePropertiesViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		public ImagePropertiesViewModel(ImageModel model, Func<string> getImagePath, IImageOperations imageOperations)
		{
			this.model = model;
			this.getImagePath = getImagePath;
			this.imageOperations = imageOperations;

			sourceField = model.Source?.OriginalString;

			ErrorCollection = new Dictionary<string, List<Exception>>();
		}

		private readonly ImageModel model;
		private readonly Func<string> getImagePath;
		private readonly IImageOperations imageOperations;

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

		public string FilePath { get { return getImagePath(); } }
		public string Name { get { return model.FileName; } }

		public string Title { get { return model.Title; } set { model.Title = value; } }
		public List<string> SubjectCollection { get { return model.SubjectCollection; } set { model.SubjectCollection = value; } }
		public List<string> ArtistCollection { get { return model.ArtistCollection; } set { model.ArtistCollection = value; } }
		public List<string> TagCollection { get { return model.TagCollection; } set { model.TagCollection = value; } }
		public int Score { get { return model.Score; } set { model.Score = value; } }

		public DateTime AdditionDate { get { return model.AdditionDate.ToLocalTime(); } }

		public string Hash { get { return model.Hash; } }
		public string FileSize { get { return FormatExtensions.FormatUnit(new FileInfo(FilePath).Length, "B"); } }
		public string Dimensions { get { return imageOperations.GetDimensions(File.ReadAllBytes(FilePath)); } }

		private string sourceField;
		public string Source
		{
			get { return sourceField; }
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					value = null;
				if (sourceField == value)
					return;

				sourceField = value;
				ErrorCollection[nameof(Source)] = new List<Exception>();

				Uri valueAsUri = null;
				if ((sourceField != null) && (Uri.TryCreate(sourceField, UriKind.Absolute, out valueAsUri) == false))
					ErrorCollection[nameof(Source)].Add(new ArgumentException("The URI is invalid.", nameof(Source)));

				if (ErrorCollection[nameof(Source)].Any() == false)
					model.Source = valueAsUri;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Source)));
			}
		}

		public List<string> AllSubjects { get; set; } = new List<string>();
		public List<string> AllArtists { get; set; } = new List<string>();
		public List<string> AllTags { get; set; } = new List<string>();

		public void NotifyFileChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hash)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSize)));
		}
	}
}
