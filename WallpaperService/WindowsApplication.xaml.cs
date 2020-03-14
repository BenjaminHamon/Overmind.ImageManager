using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using NLog;
using NLog.Common;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Queries;
using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

using JsonSerializerProxy = Overmind.ImageManager.Model.Serialization.JsonSerializer;

namespace Overmind.ImageManager.WallpaperService
{
	public partial class WindowsApplication : System.Windows.Application
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			AppDomain.CurrentDomain.UnhandledException += ReportFatalError;

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
			JsonSerializer serializerImplementation = new JsonSerializer() { Formatting = Formatting.Indented };
			JsonSerializerProxy serializer = new JsonSerializerProxy(serializerImplementation);

			applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			settingsProvider = new SettingsProvider(serializer, applicationDataDirectory);
			collectionProvider = new CollectionProvider(serializer, null, null);
			queryEngine = new LuceneQueryEngine();
			randomFactory = () => new Random();
		}

		private readonly string applicationDataDirectory;
		private readonly ICollectionProvider collectionProvider;
		private readonly SettingsProvider settingsProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly Func<Random> randomFactory;

		private MainViewModel mainViewModel;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			Logger.Info("Starting {0}", ApplicationName);

			mainViewModel = new MainViewModel(settingsProvider, collectionProvider, queryEngine, randomFactory, applicationDataDirectory);
			mainViewModel.ReloadSettings();
			mainViewModel.ApplyConfiguration();

			MainView mainView = new MainView() { DataContext = mainViewModel };

			MainWindow = new Window()
			{
				Content = mainView,
				Title = ApplicationTitle,
				Width = 300,
				SizeToContent = SizeToContent.Height,
				ResizeMode = ResizeMode.NoResize,
			};

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

		private static void ReportFatalError(object sender, UnhandledExceptionEventArgs eventArguments)
		{
			Exception exception = (Exception)eventArguments.ExceptionObject;

			InternalLogger.Fatal(exception, "Unhandled exception");
			Logger.Fatal(exception, "Unhandled exception");
			ShowError(ApplicationTitle, "A fatal error has occured.", exception);

		}

		public static void ShowError(string context, string message, Exception exception)
		{
			string formattedMessage = message;
			if (exception != null)
				formattedMessage += Environment.NewLine + FormatExtensions.FormatExceptionHint(exception);

			MessageBox.Show(formattedMessage, context, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
