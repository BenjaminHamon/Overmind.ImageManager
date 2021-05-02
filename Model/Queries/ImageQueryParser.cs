using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;

namespace Overmind.ImageManager.Model.Queries
{
	public class ImageQueryParser : QueryParser
	{
		public ImageQueryParser(Lucene.Net.Util.Version luceneVersion, string defaultField, Analyzer analyzer)
			: base(luceneVersion, defaultField, analyzer)
		{ }

		protected override Query NewTermQuery(Term term)
		{
			if (term.Field == "score")
			{
				int value = Int32.Parse(term.Text);
				return NumericRangeQuery.NewIntRange(term.Field, value, value, true, true);
			}

			return base.NewTermQuery(term);
		}

		protected override Query NewRangeQuery(string field, string part1, string part2, bool inclusive)
		{
			if (field == "score")
			{
				int minimum = Int32.Parse(part1);
				int maximum = Int32.Parse(part2);
				return NumericRangeQuery.NewIntRange(field, minimum, maximum, inclusive, inclusive);
			}

			return base.NewRangeQuery(field, part1, part2, inclusive);
		}
	}
}
