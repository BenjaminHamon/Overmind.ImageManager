using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Overmind.ImageManager.Model.Wallpapers
{
	/// <summary>
	/// Service to cycle the system wallpaper using a timer and a collection from which the next image is randomly selected.
	/// </summary>
	public class WallpaperServiceInstance : IDisposable
	{
		public static WallpaperServiceInstance CreateInstance(ReadOnlyCollectionModel collectionModel,
			WallpaperConfiguration configuration, Action<ImageModel> setSystemWallpaper, Random random)
		{
			IEnumerable<ImageModel> imageCollection = collectionModel.Images;

			if (String.IsNullOrEmpty(configuration.ImageQuery) == false)
			{
				Func<ImageModel, bool> queryFunction = collectionModel.CreateSearchQuery(configuration.ImageQuery);
				imageCollection = imageCollection.Where(queryFunction);
			}

			return new WallpaperServiceInstance(imageCollection, setSystemWallpaper, random, configuration.CyclePeriod);
		}

		public WallpaperServiceInstance(IEnumerable<ImageModel> imageCollection,
			Action<ImageModel> setSystemWallpaper, Random random, TimeSpan cyclePeriod)
		{
			this.imageCollection = (imageCollection ?? throw new ArgumentNullException(nameof(imageCollection))).ToList();
			this.setSystemWallpaper = setSystemWallpaper ?? throw new ArgumentNullException(nameof(setSystemWallpaper));
			this.random = random ?? throw new ArgumentNullException(nameof(random));
			this.cyclePeriod = cyclePeriod;

			isRunning = true;
			cycleTimer = new Timer(CycleWallpaper, null, TimeSpan.Zero, cyclePeriod);
		}

		private readonly List<ImageModel> imageCollection;
		private readonly Action<ImageModel> setSystemWallpaper;
		private readonly Random random;
		private readonly TimeSpan cyclePeriod;
		private readonly Timer cycleTimer;
		private readonly object cycleLock = new object();

		private bool isRunning;
		private ImageModel currentWallpaper;

		public void Dispose()
		{
			lock (cycleLock)
			{
				cycleTimer.Dispose();
				isRunning = false;
			}
		}
		
		public void CycleNow()
		{
			cycleTimer.Change(TimeSpan.Zero, cyclePeriod);
		}

		private void CycleWallpaper(object state)
		{
			lock (cycleLock)
			{
				if (isRunning == false)
					return;

				try
				{
					List<ImageModel> collectionCopy = imageCollection.ToList();
					ImageModel newWallpaper = null;
					if (collectionCopy.Any())
					{
						if (collectionCopy.Count == 1)
							newWallpaper = collectionCopy.Single();
						else
						{
							if (currentWallpaper != null)
								collectionCopy.Remove(currentWallpaper);
							newWallpaper = collectionCopy[random.Next(collectionCopy.Count)];
						}
					}

					if (newWallpaper != null)
					{
						System.Diagnostics.Trace.TraceInformation("Setting wallpaper to {0}", newWallpaper.FileName);

						setSystemWallpaper(newWallpaper);
						currentWallpaper = newWallpaper;
					}
				}
				catch (Exception exception)
				{
					System.Diagnostics.Trace.TraceError("[WallpaperService] Failed to cycle wallpaper: {0}",  exception);
				}
			}
		}
	}
}
