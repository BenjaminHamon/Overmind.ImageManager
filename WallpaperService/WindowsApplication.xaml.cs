using Newtonsoft.Json;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Wallpapers;
using System;
using System.IO;
using System.Windows;

namespace Overmind.ImageManager.WallpaperService
{
	public partial class WindowsApplication : System.Windows.Application
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			WindowsApplication windowsApplication = new WindowsApplication();
			windowsApplication.InitializeComponent();
			windowsApplication.Run();
		}

		public WindowsApplication()
		{
			string configurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			DataProvider dataProvider = new DataProvider(serializer);
			WallpaperConfigurationProvider configurationProvider = new WallpaperConfigurationProvider(serializer, configurationDirectory);
			serviceViewModel = new WallpaperServiceViewModel(configurationProvider, dataProvider, configurationDirectory);
		}
		
		private readonly WallpaperServiceViewModel serviceViewModel;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			MainWindow = new WallpaperServiceView() { DataContext = serviceViewModel };
			MainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			serviceViewModel.Dispose();
		}
	}
}
