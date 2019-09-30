using NLog;
using Overmind.ImageManager.Model.Queries;
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
		private static readonly Logger Logger = LogManager.GetLogger(nameof(WallpaperServiceInstance));

		public static WallpaperServiceInstance CreateInstance(WallpaperConfiguration configuration,
			CollectionProvider collectionProvider, IQueryEngine<ImageModel> queryEngine, Action<string> setSystemWallpaperFromPath, Random random)
		{
			CollectionData collectionData = collectionProvider.LoadCollection(configuration.CollectionPath);
			ReadOnlyCollectionModel collectionModel = new ReadOnlyCollectionModel(collectionProvider, collectionData, configuration.CollectionPath);
			ICollection<ImageModel> imageCollection = queryEngine.Search(collectionModel.AllImages, configuration.ImageQuery);
			Action<ImageModel> setSystemWallpaperFromImage = image => setSystemWallpaperFromPath(collectionModel.GetImagePath(image));

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
						Logger.Info("Setting wallpaper to {0}", newWallpaper.FileName);

						setSystemWallpaper(newWallpaper);
						CurrentWallpaper = newWallpaper;
					}
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "[WallpaperService] Failed to cycle wallpaper");
				}
			}
		}
	}
}
