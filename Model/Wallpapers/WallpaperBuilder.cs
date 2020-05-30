using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Overmind.ImageManager.Model.Wallpapers
{
	/// <summary>A builder for creating image files suitable for wallpaper display.</summary>
	public class WallpaperBuilder
	{
		public WallpaperBuilder(ImageFormat imageFormat, int imageQuality)
		{
			this.imageFormat = imageFormat;
			this.imageQuality = imageQuality;
		}

		private readonly ImageFormat imageFormat;
		private readonly int imageQuality;

		/// <summary>Create a wallpaper image which will use the system settings.</summary>
		public void Create(string sourcePath, string destinationPath)
		{
			using (Image sourceImage = Image.FromFile(sourcePath))
			using (Bitmap finalImage = new Bitmap(sourceImage.Width, sourceImage.Height))
			{
				Rectangle drawArea = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);

				Draw(sourceImage, finalImage, drawArea);
				Save(finalImage, destinationPath);
			}
		}

		/// <summary>Create a wallpaper image which will fit a single screen.</summary>
		public void CreateForSingleScreen(string sourcePath, string destinationPath, int displayWidth, int displayHeight)
		{
			using (Image sourceImage = Image.FromFile(sourcePath))
			using (Bitmap finalImage = new Bitmap(displayWidth, displayHeight))
			{
				Rectangle drawArea = GetDrawArea(sourceImage.Width, sourceImage.Height, displayWidth, displayHeight);

				Draw(sourceImage, finalImage, drawArea);
				Save(finalImage, destinationPath);
			}
		}

		private Rectangle GetDrawArea(int sourceImageWidth, int sourceImageHeight, int displayWidth, int displayHeight)
		{
			Rectangle drawArea = new Rectangle(0, 0, displayWidth, displayHeight);

			float sourceRatio = (float) sourceImageWidth / sourceImageHeight;
			float screenRatio = (float) displayWidth / displayHeight;

			if (sourceRatio > screenRatio)
			{
				drawArea.Width = displayWidth;
				drawArea.Height = (int) ((float) displayWidth * sourceImageHeight / sourceImageWidth);
				drawArea.Y = (displayHeight - drawArea.Height) / 2;
			}

			if (sourceRatio < screenRatio)
			{
				drawArea.Width = (int) ((float) displayHeight * sourceImageWidth / sourceImageHeight);
				drawArea.Height = displayHeight;
				drawArea.X = (displayWidth - drawArea.Width) / 2;
			}

			return drawArea;
		}

		// See https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp/24199315#24199315
		private void Draw(Image source, Bitmap destination, Rectangle drawArea)
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
