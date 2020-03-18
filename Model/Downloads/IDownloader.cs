using System;
using System.Threading;
using System.Threading.Tasks;

namespace Overmind.ImageManager.Model.Downloads
{
	public delegate void ProgressHandler(Uri uri, long progress, long totalSize);

	public interface IDownloader
	{
		Task<byte[]> Download(Uri uri, ProgressHandler progressHandler, CancellationToken cancellationToken);
		Task<DownloadSource> ResolveSource(Uri uri, DownloadSourceConfiguration sourceConfiguration, CancellationToken cancellationToken);
	}
}
