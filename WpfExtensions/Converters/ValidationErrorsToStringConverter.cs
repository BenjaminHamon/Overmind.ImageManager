using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Overmind.WpfExtensions.Converters
{
	/// <summary>Converter to turn a collection of validation errors from a control into a message to be displayed to the user.</summary>
	[ValueConversion(typeof(IEnumerable), typeof(string))]
	public class ValidationErrorsToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IEnumerable<ValidationError>)
			{
				IEnumerable<ValidationError> errorCollection = (IEnumerable<ValidationError>)value;
				if (errorCollection.Any())
					return String.Join(Environment.NewLine, errorCollection.Select(ConvertError));
			}
			else if (value is IDictionary<string, List<Exception>>)
			{
				IDictionary<string, List<Exception>> errorCollection = (IDictionary<string, List<Exception>>)value;
				if (errorCollection.Any())
					return String.Join(Environment.NewLine, errorCollection.SelectMany(kvp => kvp.Value).Select(ConvertError));
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		private string ConvertError(ValidationError error)
		{
			if (error.ErrorContent is Exception)
				return FormatExtensions.FormatExceptionSummary((Exception)error.ErrorContent);
			return error.ErrorContent.ToString();
		}

		private string ConvertError(Exception exception)
		{
			return FormatExtensions.FormatExceptionSummary(exception);
		}
	}
}
