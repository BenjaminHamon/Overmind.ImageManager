using System;
using System.Windows;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class WindowsApplication : System.Windows.Application
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			Model.Application application = new Model.Application();
			WindowsApplication windowsApplication = new WindowsApplication(application);
			windowsApplication.InitializeComponent();
			windowsApplication.Run();
		}

		public WindowsApplication(Model.Application application)
		{
			this.application = application;
		}

		private readonly Model.Application application;
		private MainViewModel mainViewModel;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			mainViewModel = new MainViewModel(application.DataProvider);
			MainWindow = new MainWindow() { DataContext = mainViewModel };
			MainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (mainViewModel != null)
			{
				mainViewModel.Dispose();
				mainViewModel = null;
			}
		}
	}
}
