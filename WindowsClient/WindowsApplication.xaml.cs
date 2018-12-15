using Newtonsoft.Json;
using NLog;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.WpfExtensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient
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

		public static string ApplicationTitle { get { return "Overmind Image Manager"; } }
		public static string ApplicationName { get { return "ImageManager"; } }
		public static string ApplicationFullName { get { return Assembly.GetExecutingAssembly().GetName().Name; } }
		public static Version ApplicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
		public static string ApplicationFullVersion { get { return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; } }

		private static readonly Logger Logger = LogManager.GetLogger(nameof(WindowsApplication));

		public WindowsApplication()
		{
			string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			FileNameFormatter fileNameFormatter = new FileNameFormatter();

			dataProvider = new DataProvider(serializer, fileNameFormatter);
			settingsProvider = new SettingsProvider(serializer, settingsDirectory);
		}

		private readonly DataProvider dataProvider;
		private readonly SettingsProvider settingsProvider;

		private MainViewModel mainViewModel;

		private Window downloaderWindow;
		private Window settingsWindow;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			Logger.Info("Starting {0}", ApplicationName);

			mainViewModel = new MainViewModel(this, dataProvider);
			MainWindow = new MainWindow() { DataContext = mainViewModel };
			MainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs eventArguments)
		{
			Logger.Info("Exiting {0}", ApplicationName);

			if (mainViewModel != null)
				mainViewModel.Dispose();
		}

		public void ShowDownloader()
		{
			if (downloaderWindow == null)
			{
				DownloaderView downloaderView = new DownloaderView();
				Binding dataContextBinding = new Binding() { Source = mainViewModel, Path = new PropertyPath(nameof(MainViewModel.Downloader)) };
				BindingOperations.SetBinding(downloaderView, FrameworkElement.DataContextProperty, dataContextBinding);

				downloaderWindow = new Window()
				{
					Title = "Downloads - " + ApplicationTitle,
					Content = downloaderView,
					Height = 400,
					Width = 600,
				};

				downloaderWindow.Closed += (s, e) => downloaderWindow = null;
				downloaderWindow.Show();
			}
			else
			{
				if (downloaderWindow.WindowState == WindowState.Minimized)
					downloaderWindow.WindowState = WindowState.Normal;
				downloaderWindow.Activate();
			}
		}

		public void ShowSettings()
		{
			if (settingsWindow == null)
			{
				SettingsViewModel settingsViewModel = new SettingsViewModel(settingsProvider);
				SettingsView settingsView = new SettingsView() { DataContext = settingsViewModel };

				settingsWindow = new Window()
				{
					Title = "Settings - " + ApplicationTitle,
					Content = settingsView,
					Height = 800,
					Width = 800,
				};

				settingsWindow.Closed += (s, e) => settingsWindow = null;
				settingsWindow.Show();
			}
			else
			{
				if (settingsWindow.WindowState == WindowState.Minimized)
					settingsWindow.WindowState = WindowState.Normal;
				settingsWindow.Activate();
			}
		}

		public void ViewImage(ImageViewModel image)
		{
			Window imageWindow = new ImageView() { DataContext = image };

			// Use BeginInvoke so that the call finishes before the window is shown
			// to ensure the window is correctly activated and in the foreground
			// See https://stackoverflow.com/questions/14055794/wpf-treeview-restores-its-focus-after-double-click/14077266
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => imageWindow.Show()));
		}

		public void OpenImageExternally(ImageViewModel image, string mode)
		{
			ProcessStartInfo processInformation = new ProcessStartInfo() { FileName = image.FilePath, Verb = mode };

			try
			{
				using (Process process = Process.Start(processInformation)) { }
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to open image in external process (Path: '{0}')", image.FilePath);
			}
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
