using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using NLog;
using Overmind.ImageManager.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Overmind.ImageManager.WallpaperService
{
	public partial class WindowsApplication : System.Windows.Application
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			AppDomain.CurrentDomain.UnhandledException += (s, e) => Logger.Fatal((Exception)e.ExceptionObject, "Unhandled exception");

			string applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			Model.Application.InitializeLogging(ApplicationFullName, ApplicationName, applicationDataDirectory);

			Logger.Info("------------------------------");
			Logger.Info("Initializing {0} (Version: {1})", ApplicationName, ApplicationFullVersion);

			WindowsApplication windowsApplication = new WindowsApplication();
			windowsApplication.InitializeComponent();
			windowsApplication.Run();
		}

		public static string ApplicationTitle { get { return "Overmind Wallpaper Service"; } }
		public static string ApplicationName { get { return "WallpaperService"; } }
		public static string ApplicationFullName { get { return Assembly.GetExecutingAssembly().GetName().Name; } }
		public static Version ApplicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
		public static string ApplicationFullVersion { get { return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; } }

		private static readonly Logger Logger = LogManager.GetLogger(nameof(WindowsApplication));

		public WindowsApplication()
		{
			string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			DataProvider dataProvider = new DataProvider(serializer, null);
			SettingsProvider settingsProvider = new SettingsProvider(serializer, settingsDirectory);
			mainViewModel = new MainViewModel(settingsProvider, dataProvider, settingsDirectory);
		}

		private readonly MainViewModel mainViewModel;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			Logger.Info("Starting {0}", ApplicationName);

			mainViewModel.ReloadSettings();
			mainViewModel.ApplyConfiguration();

			MainWindow = new MainView() { DataContext = mainViewModel };
			MainWindow.Closing += HideMainWindow;

			// FIXME: The NotificationIcon bindings throw exceptions on initialization (but still work).
			TaskbarIcon notificationIcon = (TaskbarIcon)Resources["NotificationIcon"];
			notificationIcon.DataContext = mainViewModel;
		}

		private void Application_Exit(object sender, ExitEventArgs eventArguments)
		{
			Logger.Info("Exiting {0}", ApplicationName);

			mainViewModel.Dispose();
		}

		private void ShowMainWindow(object sender, RoutedEventArgs eventArguments)
		{
			MainWindow.Show();
			MainWindow.Activate();
		}

		private void HideMainWindow(object sender, CancelEventArgs eventArguments)
		{
			MainWindow.Hide();
			eventArguments.Cancel = true;
		}

		private void ExitApplication(object sender, RoutedEventArgs eventArguments)
		{
			Shutdown();
		}
	}
}
