using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Overmind.ImageManager.WindowsClient
{
	// This converter avoids holding a lock on the image file.
	[ValueConversion(typeof(string), typeof(ImageSource))]
	public class StringToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string filePath = value as string;
			if ((filePath != null) && File.Exists(filePath))
			{
				BitmapImage image = new BitmapImage();
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.StreamSource = stream;
					image.EndInit();
				}
				return image;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
