using Microsoft.Win32;
using System.ComponentModel;
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

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

		/// <summary>Retrieves the file path for the active wallpaper.</summary>
		public static string Get()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			return (string) key.GetValue("Wallpaper");
		}

		/// <summary>Sets the wallpaper.</summary>
		public static void Set(string filePath)
		{
			int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
			if (result == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
