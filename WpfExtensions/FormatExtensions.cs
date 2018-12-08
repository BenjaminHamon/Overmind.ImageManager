using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.WpfExtensions
{
	public static class FormatExtensions
	{
		/// <summary>Format a summary from an exception hierarchy by using the first line of each exception.</summary>
		public static string FormatExceptionSummary(Exception exception)
		{
			ICollection<Exception> innerExceptions = new List<Exception>();
			if (exception is AggregateException)
				innerExceptions = ((AggregateException)exception).InnerExceptions;
			else if (exception.InnerException != null)
				innerExceptions.Add(exception.InnerException);

			string formattedMessage = FormatExceptionHint(exception);
			if (innerExceptions.Any())
				formattedMessage += Environment.NewLine + String.Join(Environment.NewLine, innerExceptions.Select(FormatExceptionSummary));
			return formattedMessage;
		}

		/// <summary>Format a hint from an exception by using its message first line.</summary>
		public static string FormatExceptionHint(Exception exception)
		{
			return exception.Message.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).First();
		}
	}
}
