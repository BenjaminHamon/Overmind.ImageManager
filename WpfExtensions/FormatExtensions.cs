using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.WpfExtensions
{
	public static class FormatExtensions
	{
		// See https://en.wikipedia.org/wiki/Metric_prefix
		private static readonly List<string> PrefixCollection = new List<string>() { "", "k", "M", "G", "T", "P", "E", "Z", "Y" };

		/// <summary>Format a numeric value to a human-friendly representation, using prefixes from the International System of Units.</summary>
		/// <param name="value">The value to format.</param>
		/// <param name="unit">The symbol of the unit of measurement for the provided value.</param>
		/// <param name="format">A numeric format string, passed to String.Format.</param>
		/// <param name="formatProvider">An object that supplies culture-specific formatting information, passed to String.Format.</param>
		/// <returns>The human-friendly representation of the value.</returns>
		public static string FormatUnit(double value, string unit, string format, IFormatProvider formatProvider = null)
		{
			double multiplier = 1000;

			if ((unit == "B") || (unit == "B/s"))
				multiplier = 1024;

			int prefixIndex = 0;

			while ((Math.Abs(value) > multiplier) && (prefixIndex < (PrefixCollection.Count - 1)))
			{
				value /= multiplier;
				prefixIndex += 1;
			}

			return value.ToString(format, formatProvider) + " " + PrefixCollection[prefixIndex] + unit;
		}

		/// <summary>Format a summary from an exception hierarchy by using the first line of each exception.</summary>
		public static string FormatExceptionSummary(Exception exception)
		{
			ICollection<Exception> innerExceptions = new List<Exception>();

			if (exception is AggregateException)
			{
				innerExceptions = ((AggregateException)exception).InnerExceptions;
			}
			else if (exception.InnerException != null)
			{
				innerExceptions.Add(exception.InnerException);
			}

			string formattedMessage = FormatExceptionHint(exception);

			if (innerExceptions.Any())
			{
				formattedMessage += Environment.NewLine + String.Join(Environment.NewLine, innerExceptions.Select(FormatExceptionSummary));
			}

			return formattedMessage;
		}

		/// <summary>Format a hint from an exception by using its message first line.</summary>
		public static string FormatExceptionHint(Exception exception)
		{
			return exception.Message.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).First();
		}
	}
}
