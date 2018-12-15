using System;
using System.Collections.Generic;

namespace Overmind.ImageManager.Model.Queries
{
	/// <summary>Interface for an engine performing queries on data sets.</summary>
	/// <typeparam name="TData">The type of object for which the engine perform queries.</typeparam>
	public interface IQueryEngine<TData>
	{
		/// <summary>Assert that a query string is considered valid by the engine.</summary>
		/// <exception cref="ArgumentException">Thrown if the query string is invalid.</exception>
		void AssertQuery(string queryString);

		/// <summary>Perform a search on the given data set.</summary>
		/// <returns>The search results as a collection of objects from the data set.</returns>
		ICollection<TData> Search(IEnumerable<TData> dataSet, string queryString);
	}
}
