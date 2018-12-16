using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Extensions
{
	public static class EnumerableExtensions
	{
		public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource value)
		{
			int index = 0;

			foreach (TSource item in source)
			{
				if (EqualityComparer<TSource>.Default.Equals(item, value))
					return index;

				index++;
			}

			return -1;
		}

		// See https://stackoverflow.com/questions/5807128/an-extension-method-on-ienumerable-needed-for-shuffling
		public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source, Random random)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (random == null) throw new ArgumentNullException(nameof(random));

			return source.ShuffleIterator(random);
		}

		private static IEnumerable<TSource> ShuffleIterator<TSource>(this IEnumerable<TSource> source, Random random)
		{
			List<TSource> buffer = source.ToList();

			for (int currentIndex = 0; currentIndex < buffer.Count; currentIndex++)
			{
				int randomIndex = random.Next(currentIndex, buffer.Count);
				yield return buffer[randomIndex];
				buffer[randomIndex] = buffer[currentIndex];
			}
		}
	}
}
