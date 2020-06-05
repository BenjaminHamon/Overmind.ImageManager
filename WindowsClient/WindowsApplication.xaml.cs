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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
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

			CookieContainer cookieContainer = new CookieContainer();
			httpClient = new HttpClient(new HttpClientHandler() { CookieContainer = cookieContainer });
			httpClient.DefaultRequestHeaders.Add("User-Agent", ApplicationFullName + "/" + ApplicationFullVersion);

			hashAlgorithm = MD5.Create();
			imageOperations = new ImageOperations(hashAlgorithm);

			settingsProvider = new SettingsProvider(serializer, applicationDataDirectory);
			collectionProvider = new CollectionProvider(serializer, imageOperations, fileNameFormatter);
			queryEngine = new LuceneQueryEngine();
			downloader = new Downloader(httpClient);
			randomFactory = () => new Random();
		}

		private readonly HttpClient httpClient;
		private readonly HashAlgorithm hashAlgorithm;
		private readonly SettingsProvider settingsProvider;
		private readonly ICollectionProvider collectionProvider;
		private readonly IImageOperations imageOperations;
		private readonly IQueryEngine<ImageModel> queryEngine;
		private readonly IDownloader downloader;
		private readonly Func<Random> randomFactory;

		private MainViewModel mainViewModel;

		private CustomWindow downloaderWindow;
		private CustomWindow settingsWindow;

		private void Application_Startup(object sender, StartupEventArgs eventArguments)
		{
			Logger.Info("Starting {0}", ApplicationName);

			mainViewModel = new MainViewModel(this, settingsProvider, collectionProvider, imageOperations, queryEngine, downloader, randomFactory);

			MainMenuView mainMenuView = new MainMenuView() { DataContext = mainViewModel };
			MainView mainView = new MainView() { DataContext = mainViewModel };
			mainMenuView.CollectionView = mainView.CollectionView;

			MainWindow = new CustomWindow();
			((CustomWindow)MainWindow).Menu.Content = mainMenuView;
			((CustomWindow)MainWindow).MainContent.Content = mainView;

			Binding titleBinding = new Binding()
			{
				Source = mainViewModel,
				Path = new PropertyPath("ActiveCollection.Name"),
				StringFormat = "{0}" + " - " + ApplicationTitle,
				FallbackValue = ApplicationTitle,
			};

			BindingOperations.SetBinding(MainWindow, Window.TitleProperty, titleBinding);

			mainMenuView.RegisterCommands(MainWindow);

			MainWindow.Deactivated += (s, e) => Dispatcher.BeginInvoke(new Action(() => ForceUpdateOnFocusedElement(mainView)));
			MainWindow.Closing += mainMenuView.ExitApplication;
			MainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs eventArguments)
		{
			Logger.Info("Exiting {0}", ApplicationName);

			if (mainViewModel != null)
				mainViewModel.Dispose();

			hashAlgorithm.Dispose();
			httpClient.Dispose();
		}

		public void ShowDownloader()
		{
			if (downloaderWindow == null)
			{
				DownloaderView downloaderView = new DownloaderView();
				Binding dataContextBinding = new Binding() { Source = mainViewModel, Path = new PropertyPath(nameof(MainViewModel.Downloader)) };
				BindingOperations.SetBinding(downloaderView, FrameworkElement.DataContextProperty, dataContextBinding);

				downloaderWindow = new CustomWindow()
				{
					Title = "Download" +  " - " + ApplicationTitle,
					Height = 400,
					Width = 600,
				};

				downloaderWindow.MainContent.Content = downloaderView;

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

				settingsWindow = new CustomWindow()
				{
					Title = "Settings" + " - " + ApplicationTitle,
					Height = 800,
					Width = 800,
				};

				settingsWindow.MainContent.Content = settingsView;

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
				Title = "About" + " " + ApplicationTitle,
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

			CustomWindow window = new CustomWindow() { Title = "Slide Show" };
			window.TitleTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
			window.MainContent.Content = slideShowView;

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

		public static void ForceUpdateOnFocusedElement(FrameworkElement element)
		{
			// Some controls trigger the data binding update source when they lose the focus, but this does not happen when switching focus scope.
			// The issue occurs when using a menu command, changing window or closing the window (directly or from the system task bar).

			// See also https://stackoverflow.com/questions/57493/wpf-databind-before-saving
			// - Disabling the menu focus scope is only a partial fix since it does not catch the issue when the main window is closed,
			//   plus it introduces a bug where the main window close button may require two clicks, from the focus being stuck on the menu somehow
			//   (the behavior is similar to having a menu open, the first click would dismiss the menu and does not trigger on the button)
			// - Updating the active element directly relies on Keyboard.FocusedElement, which is null when closing the window from the system task bar,
			//   and requires supporting any control type (TokenListView does not expose its internal update logic).

			Window mainWindow = Window.GetWindow(element);
			IInputElement focusedElement = FocusManager.GetFocusedElement(mainWindow);

			if (focusedElement != null)
			{
				FocusManager.SetFocusedElement(mainWindow, element);
				FocusManager.SetFocusedElement(mainWindow, focusedElement);
			}
		}
	}
}
