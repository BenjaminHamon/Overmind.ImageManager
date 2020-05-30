using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Overmind.ImageManager.WallpaperService
{
	public class WallpaperBuilder
	{
		public WallpaperBuilder(ImageFormat imageFormat, int imageQuality)
		{
			this.imageFormat = imageFormat;
			this.imageQuality = imageQuality;
		}

		private readonly ImageFormat imageFormat;
		private readonly int imageQuality;

		public void Create(string sourcePath, string destinationPath, int screenWidth, int screenHeight)
		{
			using (Image sourceImage = Image.FromFile(sourcePath))
			{
				Rectangle drawArea = GetDrawArea(sourceImage.Width, sourceImage.Height, screenWidth, screenHeight);

				using (Bitmap finalImage = new Bitmap(drawArea.Width, drawArea.Height))
				{
					DrawResized(sourceImage, finalImage, drawArea);
					Save(finalImage, destinationPath);
				}
			}
		}

		private Rectangle GetDrawArea(int sourceImageWidth, int sourceImageHeight, int screenWidth, int screenHeight)
		{
			int destinationWidth = screenWidth;
			int destinationHeight = screenHeight;

			float sourceRatio = (float) sourceImageWidth / sourceImageHeight;
			float screenRatio = (float) screenWidth / screenHeight;

			if (sourceRatio > screenRatio)
			{
				destinationWidth = screenWidth;
				destinationHeight = (int) ((float) screenWidth * sourceImageHeight / sourceImageWidth);
			}

			if (sourceRatio < screenRatio)
			{
				destinationWidth = (int) ((float) screenHeight * sourceImageWidth / sourceImageHeight);
				destinationHeight = screenHeight;
			}

			return new Rectangle(0, 0, destinationWidth, destinationHeight);
		}

		// See https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp/24199315#24199315
		private void DrawResized(Image source, Bitmap destination, Rectangle drawArea)
		{
			destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

			using (Graphics graphics = Graphics.FromImage(destination))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (ImageAttributes imageAttributes = new ImageAttributes())
				{
					imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(source, drawArea, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
				}
			}
		}

		private void Save(Image image, string outputPath)
		{
			ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == imageFormat.Guid);
			using (EncoderParameters encoderParameters = new EncoderParameters())
			{
				encoderParameters.Param = new[] { new EncoderParameter(Encoder.Quality, imageQuality) };
				image.Save(outputPath, encoder, encoderParameters);
			}
		}
	}
}
