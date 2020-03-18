﻿using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImagePropertiesViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		public ImagePropertiesViewModel(ImageModel model, Func<string> getImagePath, IImageOperations imageOperations, Dispatcher dispatcher)
		{
			this.model = model;
			this.getImagePath = getImagePath;
			this.imageOperations = imageOperations;
			this.dispatcher = dispatcher;

			sourceUriField = model.Source.Uri?.OriginalString;
			Format = GetFormatFromFilePath();

			ErrorCollection = new Dictionary<string, List<Exception>>();

			Task.Run(RefreshFileProperties);
		}

		private readonly ImageModel model;
		private readonly Func<string> getImagePath;
		private readonly IImageOperations imageOperations;
		private readonly Dispatcher dispatcher;
		private readonly object refreshLock = new object();

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

		public List<string> AllSubjects { get; set; } = new List<string>();
		public List<string> AllArtists { get; set; } = new List<string>();
		public List<string> AllTags { get; set; } = new List<string>();

		private string sourceUriField;
		public string SourceUri
		{
			get { return sourceUriField; }
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					value = null;
				if (sourceUriField == value)
					return;

				sourceUriField = value;
				ErrorCollection[nameof(SourceUri)] = new List<Exception>();

				Uri valueAsUri = null;
				if ((sourceUriField != null) && (Uri.TryCreate(sourceUriField, UriKind.Absolute, out valueAsUri) == false))
					ErrorCollection[nameof(SourceUri)].Add(new ArgumentException("The URI is invalid.", nameof(SourceUri)));

				if (ErrorCollection[nameof(SourceUri)].Any() == false)
					model.Source.Uri = valueAsUri;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceUri)));
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SourceUri)));
			}
		}

		public string SourceFileName { get { return model.Source.FileName; } }
		public string SourceTitle { get { return model.Source.Title; } }
		public string SourceHash { get { return model.Source.Hash; } }
		public DateTime AdditionDate { get { return model.AdditionDate.ToLocalTime(); } }

		public void NotifySourceChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceUri)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceFileName)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceTitle)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceHash)));
		}

		public string Hash { get { return model.Hash; } }
		public string Format { get; private set; }
		public long FileSize { get; private set; }
		public string Dimensions { get; private set; }

		public string FileSizeFormatted
		{
			get
			{
				if (FileSize == 0)
					return "0 B";
				return FormatExtensions.FormatUnit(FileSize, "B", "N1");
			}
		}

		public void NotifyFileChanged()
		{
			Task.Run(RefreshFileProperties);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hash)));
		}

		private void RefreshFileProperties()
		{
			byte[] imageData;

			try
			{
				imageData = File.ReadAllBytes(FilePath);
			}
			catch (SystemException)
			{
				return;
			}

			string formatResult = imageOperations.GetFormat(imageData).ToUpperInvariant();
			string dimensionsResult = imageOperations.GetDimensions(imageData);

			lock (refreshLock)
			{
				this.Format = formatResult;
				this.FileSize = imageData.Length;
				this.Dimensions = dimensionsResult;
			}

			dispatcher.Invoke(() =>
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Format)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSize)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSizeFormatted)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Dimensions)));
			});
		}

		private string GetFormatFromFilePath()
		{
			string format = Path.GetExtension(FilePath).TrimStart('.').ToUpperInvariant();

			if (format == "JPG")
				return "JPEG";

			return format;
		}
	}
}
