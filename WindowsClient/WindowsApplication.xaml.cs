using Newtonsoft.Json;
using NLog;
using NLog.Common;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.Model.Downloads;
using Overmind.ImageManager.Model.Queries;
using Overmind.ImageManager.WindowsClient.Downloads;
using Overmind.ImageManager.WindowsClient.Extensions;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using JsonSerializerProxy = Overmind.ImageManager.Model.Serialization.JsonSerializer;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class WindowsApplication : System.Windows.Application
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			AppDomain.CurrentDomain.UnhandledException += ReportFatalError;
			TaskScheduler.UnobservedTaskException += (sender, eventArguments)
				=> { Logger.Error(eventArguments.Exception, "Unhandled exception in task"); };

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
		public static string ApplicationCopyright { get { return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright; } }

		public static string InstallationDirectory { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }
		public static Uri DocumentationHome { get { return new Uri("https://github.com/BenjaminHamon/Overmind.ImageManager/wiki/"); } }

		private static readonly Logger Logger = LogManager.GetLogger(nameof(WindowsApplication));

		public WindowsApplication()
		{
			string applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Model.Application.Identifier);
			JsonSerializer serializerImplementation = new JsonSerializer() { Formatting = Formatting.Indented };
			JsonSerializerProxy serializer = new JsonSerializerProxy(serializerImplementation);
			FileNameFormatter fileNameFormatter = new FileNameFormatter();

			httpClient = new HttpClient();
			settingsProvider = new SettingsProvider(serializer, applicationDataDirectory);
			collectionProvider = new CollectionProvider(serializer, fileNameFormatter);
			queryEngine = new LuceneQueryEngine();
			downloader = new Downloader(httpClient);
			randomFactory = () => new Random();
		}

		private readonly HttpClient httpClient;
		private readonly SettingsProvider settingsProvider;
		private readonly ICollectionProvider collectionProvider;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly IDownloader downloader;
		private readonly Func<Random> randomFactory;

		private MainViewModel mainViewModel;
		private MainView mainView;

		private Window downloaderWindow;
		private Window settingsWindow;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			Logger.Info("Starting {0}", ApplicationName);

			mainViewModel = new MainViewModel(this, collectionProvider, queryEngine, downloader, randomFactory);
			mainView = new MainView() { DataContext = mainViewModel };
			MainWindow = new Window() { Content = mainView };

			Binding titleBinding = new Binding() { Source = mainViewModel, Path = new PropertyPath(nameof(MainViewModel.WindowTitle)) };
			BindingOperations.SetBinding(MainWindow, Window.TitleProperty, titleBinding);

			MainWindow.Closing += MainWindow_Closing;
			MainWindow.Show();

			mainView.Focus();
		}

		private void Application_Exit(object sender, ExitEventArgs eventArguments)
		{
			Logger.Info("Exiting {0}", ApplicationName);

			if (mainViewModel != null)
				mainViewModel.Dispose();

			httpClient.Dispose();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs eventArguments)
		{
			mainView.ExitApplication(sender, eventArguments);
			if (eventArguments.Cancel == false)
				MainWindow.Closing -= MainWindow_Closing;
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
				SettingsViewModel settingsViewModel = new SettingsViewModel(settingsProvider, queryEngine);
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

		public void ShowAbout()
		{
			AboutView aboutView = new AboutView();

			Window aboutWindow = new Window()
			{
				Title = "About " + ApplicationTitle,
				Content = aboutView,
				Owner = MainWindow,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				SizeToContent = SizeToContent.WidthAndHeight,
				ResizeMode = ResizeMode.NoResize,
			};

			aboutWindow.KeyDown += (s, e) => { if (e.Key == Key.Escape) aboutWindow.Close(); };

			aboutWindow.ShowDialog();
		}

		public void ViewImage(ImageViewModel image)
		{
			Window imageWindow = new ImageView() { DataContext = image };

			// Use BeginInvoke so that the call finishes before the window is shown
			// to ensure the window is correctly activated and in the foreground
			// See https://stackoverflow.com/questions/14055794/wpf-treeview-restores-its-focus-after-double-click/14077266
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => imageWindow.Show()));
		}

		public void SpawnSlideShow(IEnumerable<ImageViewModel> imageCollection, bool shuffle)
		{
			List<ImageViewModel> source = new List<ImageViewModel>(shuffle ? imageCollection.Shuffle(randomFactory()) : imageCollection);
			SlideShowViewModel slideShowViewModel = new SlideShowViewModel(source);
			SlideShowView slideShowView = new SlideShowView() { DataContext = slideShowViewModel };

			Window window = new Window() { Title = "Slide Show", Content = slideShowView };
			window.Show();

			slideShowView.Focus();
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

		public static void ShowDocumentation(string page)
		{
			Uri uri = new Uri(DocumentationHome, page);

			using (Process process = Process.Start(uri.ToString())) { }
		}
	}
}
