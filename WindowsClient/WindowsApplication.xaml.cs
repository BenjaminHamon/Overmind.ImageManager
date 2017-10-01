using Newtonsoft.Json;
using Overmind.ImageManager.Model;
using System;
using System.Windows;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class WindowsApplication : System.Windows.Application
	{
		public const string Name = "Overmind Image Manager";

		[STAThread]
		public static void Main(string[] arguments)
		{
			WindowsApplication windowsApplication = new WindowsApplication();
			windowsApplication.InitializeComponent();
			windowsApplication.Run();
		}

		public WindowsApplication()
		{
			JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			DataProvider dataProvider = new DataProvider(serializer);
			mainViewModel = new MainViewModel(dataProvider);
		}
		
		private readonly MainViewModel mainViewModel;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow = new MainWindow() { DataContext = mainViewModel };
			MainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (mainViewModel != null)
				mainViewModel.Dispose();
		}
	}
}
