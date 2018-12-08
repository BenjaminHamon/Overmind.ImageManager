using System;
using System.Globalization;
using System.Windows.Data;

namespace Overmind.WpfExtensions.Converters
{
	/// <summary>Converter to turn an exception into a one line message to be displayed to the user.</summary>
	[ValueConversion(typeof(Exception), typeof(string))]
	public class ExceptionToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return FormatExtensions.FormatExceptionHint((Exception)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
