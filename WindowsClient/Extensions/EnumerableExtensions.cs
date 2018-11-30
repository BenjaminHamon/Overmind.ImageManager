using System;
using System.Collections.Generic;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient.Extensions
{
	public static class EnumerableExtensions
	{
		// See https://stackoverflow.com/questions/5807128/an-extension-method-on-ienumerable-needed-for-shuffling
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (random == null) throw new ArgumentNullException(nameof(random));

			return source.ShuffleIterator(random);
		}

		private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random random)
		{
			List<T> buffer = source.ToList();
			for (int currentIndex = 0; currentIndex < buffer.Count; currentIndex++)
			{
				int randomIndex = random.Next(currentIndex, buffer.Count);
				yield return buffer[randomIndex];
				buffer[randomIndex] = buffer[currentIndex];
			}
		}
	}
}
