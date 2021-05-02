using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.Model.Queries
{
	public class LuceneQueryEngine : IQueryEngine<ImageModel>
	{
		private const Lucene.Net.Util.Version SearchVersion = Lucene.Net.Util.Version.LUCENE_30;

		public void AssertQuery(string queryString)
		{
			if (String.IsNullOrWhiteSpace(queryString))
				return;

			try
			{
				using (Analyzer searchAnalyser = new StandardAnalyzer(SearchVersion))
				{
					ImageQueryParser queryParser = new ImageQueryParser(SearchVersion, "any", searchAnalyser);
					queryParser.AllowLeadingWildcard = true;
					queryParser.Parse(queryString);
				}
			}
			catch (ParseException exception)
			{
				throw new ArgumentException("The query is invalid.", nameof(queryString), exception);
			}
		}

		public ICollection<ImageModel> Search(IEnumerable<ImageModel> dataSet, string queryString)
		{
			if (String.IsNullOrWhiteSpace(queryString))
				return new List<ImageModel>(dataSet);

			List<string> resultHashes;

			using (RAMDirectory searchIndex = new RAMDirectory())
			{
				Query query;

				using (Analyzer searchAnalyser = new StandardAnalyzer(SearchVersion))
				{
					using (IndexWriter indexWriter = new IndexWriter(searchIndex, searchAnalyser, IndexWriter.MaxFieldLength.UNLIMITED))
					{
						foreach (ImageModel image in dataSet)
							indexWriter.AddDocument(image.ToDocument());
					}

					ImageQueryParser queryParser = new ImageQueryParser(SearchVersion, "any", searchAnalyser);
					queryParser.AllowLeadingWildcard = true;
					query = queryParser.Parse(queryString);
				}

				using (IndexSearcher searcher = new IndexSearcher(searchIndex))
				{
					resultHashes = searcher.Search(query, Int32.MaxValue).ScoreDocs
						.Select(result => searcher.Doc(result.Doc).GetField("hash").StringValue)
						.ToList();
				}
			}

			Dictionary<string, ImageModel> dataSetAsDictionary = dataSet.ToDictionary(image => image.Hash, x => x);
			return resultHashes.Select(hash => dataSetAsDictionary[hash]).ToList();
		}
	}
}
