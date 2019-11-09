using NLog;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Overmind.ImageManager.WindowsClient
{
	// The image is loaded manually to avoid keeping a lock on the source file.
	// Setting a maximum size avoids keeping large images in memory for scaled down images like thumbnails.
	// Unfortunately, we need to load the image twice to know its actual size.

	[ValueConversion(typeof(string), typeof(ImageSource))]
	public class StringToImageConverter : IValueConverter
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(WindowsApplication));

		public int? MaxHeight { get; set; }
		public int? MaxWidth { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string filePath = value as string;

			if ((filePath != null) && File.Exists(filePath))
			{
				try
				{
					using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
					{
						BitmapImage image = new BitmapImage();
						image.BeginInit();
						image.CacheOption = BitmapCacheOption.None;
						image.CreateOptions = BitmapCreateOptions.None;
						image.StreamSource = stream;
						image.EndInit();
						image.Freeze();

						float heightRatio = System.Convert.ToSingle(MaxHeight ?? image.PixelHeight) / image.PixelHeight;
						float widthRatio = System.Convert.ToSingle(MaxWidth ?? image.PixelWidth) / image.PixelWidth;

						stream.Seek(0, SeekOrigin.Begin);

						image = new BitmapImage();
						image.BeginInit();
						image.CacheOption = BitmapCacheOption.OnLoad;
						if ((MaxHeight != null) && (heightRatio < 1) && (heightRatio < widthRatio))
							image.DecodePixelHeight = MaxHeight.Value;
						else if ((MaxWidth != null) && (widthRatio < 1))
							image.DecodePixelWidth = MaxWidth.Value;
						image.StreamSource = stream;
						image.EndInit();
						image.Freeze();

						return image;
					}
				}
				catch (Exception exception)
				{
					Logger.Warn(exception, "Failed to load image file: '{0}'", filePath);
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
