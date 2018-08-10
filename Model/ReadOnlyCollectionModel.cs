using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model
{
	public class ReadOnlyCollectionModel
	{
		private const Lucene.Net.Util.Version searchVersion = Lucene.Net.Util.Version.LUCENE_30;

		public ReadOnlyCollectionModel(DataProvider dataProvider, CollectionData data, string storagePath)
		{
			this.dataProvider = dataProvider;
			this.data = data;
			this.storagePath = storagePath;
		}

		protected readonly DataProvider dataProvider;
		protected readonly CollectionData data;
		protected readonly string storagePath;

		public string StoragePath { get { return storagePath; } }
		public IEnumerable<ImageModel> AllImages { get { return data.Images; } }

		public string GetImagePath(ImageModel image)
		{
			return dataProvider.GetImagePath(storagePath, image);
		}

		public IEnumerable<ImageModel> SearchSimple(string queryString)
		{
			List<Regex> queryRegexes = queryString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(element => new Regex("^" + Regex.Escape(element).Replace("\\*", ".*") + "$")).ToList();
			return AllImages.Where(image => queryRegexes.All(regex => image.GetSearchableValues().Any(value => regex.IsMatch(value))));
		}
		
		public IEnumerable<ImageModel> SearchAdvanced(string queryString)
		{
			List<string> resultHashes;

			using (RAMDirectory searchIndex = new RAMDirectory())
			{
				Query query;

				using (Analyzer searchAnalyser = new StandardAnalyzer(searchVersion))
				{
					using (IndexWriter indexWriter = new IndexWriter(searchIndex, searchAnalyser, IndexWriter.MaxFieldLength.UNLIMITED))
					{
						foreach (ImageModel image in AllImages)
							indexWriter.AddDocument(image.ToDocument());
					}

					ImageQueryParser queryParser = new ImageQueryParser(searchVersion, "any", searchAnalyser);
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

			return AllImages.Where(image => resultHashes.Contains(image.Hash));
		}
	}
}
