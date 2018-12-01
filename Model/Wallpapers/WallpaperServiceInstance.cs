using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Overmind.ImageManager.Model.Wallpapers
{
	/// <summary>
	/// Service to cycle the system wallpaper using a timer and a collection from which the next image is randomly selected.
	/// </summary>
	public class WallpaperServiceInstance : IDisposable
	{
		public static WallpaperServiceInstance CreateInstance(DataProvider dataProvider,
			WallpaperConfiguration configuration, Action<string> setSystemWallpaperFromPath, Random random)
		{
			CollectionData collectionData = dataProvider.LoadCollection(configuration.CollectionPath);
			ReadOnlyCollectionModel collectionModel = new ReadOnlyCollectionModel(dataProvider, collectionData, configuration.CollectionPath);
			Action<ImageModel> setSystemWallpaperFromImage = image => setSystemWallpaperFromPath(collectionModel.GetImagePath(image));

			IEnumerable<ImageModel> imageCollection = collectionModel.AllImages;
			if (String.IsNullOrEmpty(configuration.ImageQuery) == false)
				imageCollection = collectionModel.SearchAdvanced(configuration.ImageQuery);

			return new WallpaperServiceInstance(imageCollection, setSystemWallpaperFromImage, random, configuration.CyclePeriod);
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
		public ImageModel CurrentWallpaper { get; private set; }

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
							if (CurrentWallpaper != null)
								collectionCopy.Remove(CurrentWallpaper);
							newWallpaper = collectionCopy[random.Next(collectionCopy.Count)];
						}
					}

					if (newWallpaper != null)
					{
						Trace.TraceInformation("Setting wallpaper to {0}", newWallpaper.FileName);

						setSystemWallpaper(newWallpaper);
						CurrentWallpaper = newWallpaper;
					}
				}
				catch (Exception exception)
				{
					Trace.TraceError("[WallpaperService] Failed to cycle wallpaper: {0}",  exception);
				}
			}
		}
	}
}
