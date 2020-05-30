using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Overmind.ImageManager.WallpaperService
{
	public static class WindowsWallpaper
	{
		// See https://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net

		// See https://msdn.microsoft.com/en-us/library/ms724947.aspx
		private const int SPI_SETDESKWALLPAPER = 0x0014;
		private const int SPIF_UPDATEINIFILE = 0x01;
		private const int SPIF_SENDWININICHANGE = 0x02;

		// See https://msdn.microsoft.com/en-us/library/ms534413.aspx
		private static short PropertyTagTypeRational = 5;
		private static int PropertyTagXResolution = 0x011A;
		private static int PropertyTagYResolution = 0x011B;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

		/// <summary>Retrieves the file path for the active wallpaper.</summary>
		public static string Get()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			return (string)key.GetValue("Wallpaper");
		}

		/// <summary>Sets the wallpaper.</summary>
		public static void Set(string filePath)
		{
			int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
			if (result == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		/// <summary>Saves an image to a new file which can be set as wallpaper.</summary>
		/// <param name="quality">Image quality, between 0 and 100.</param>
		public static void Save(string sourcePath, string destinationPath, ImageFormat format, int quality)
		{
			using (Image image = Image.FromFile(sourcePath))
			{
				if (format == ImageFormat.Jpeg)
				{
					// Setting the wallpaper fails with no error code when using a jpeg image missing these properties
					if (image.PropertyIdList.Contains(PropertyTagXResolution) == false)
						AddResolutionProperty(image, PropertyTagXResolution, image.HorizontalResolution);
					if (image.PropertyIdList.Contains(PropertyTagYResolution) == false)
						AddResolutionProperty(image, PropertyTagYResolution, image.VerticalResolution);
				}

				ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == format.Guid);
				using (EncoderParameters encoderParameters = new EncoderParameters())
				{
					encoderParameters.Param = new[] { new EncoderParameter(Encoder.Quality, quality) };
					image.Save(destinationPath, encoder, encoderParameters);
				}
			}
		}

		private static void AddResolutionProperty(Image image, int id, float value)
		{
			// Convert resolution from float to rational
			byte[] propertyValue = new byte[8];
			BitConverter.GetBytes((int)value).CopyTo(propertyValue, 0); // Numerator
			BitConverter.GetBytes(1).CopyTo(propertyValue, 4); // Denominator

			// HACK: It seems to be the only way to create a new property item
			PropertyItem newProperty = image.PropertyItems.First();

			newProperty.Id = id;
			newProperty.Type = PropertyTagTypeRational;
			newProperty.Len = propertyValue.Length;
			newProperty.Value = propertyValue;
			image.SetPropertyItem(newProperty);
		}
	}
}