using System;

namespace Overmind.ImageManager.Model.Downloads
{
	public class DownloadSource
	{
		public Uri WebUri { get; set; }
		public Uri DownloadUri { get; set; }
		public string FileName { get; set; }
		public string Title { get; set; }
	}
}
