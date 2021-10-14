using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials;
using OWCE.DependencyInterfaces;
using OWCE.Pages;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using OWCE.Utils;
using OWCE.PropertyChangeHandlers;

[assembly: ExportFont("SairaExtraCondensed-Black.ttf")]
[assembly: ExportFont("SairaExtraCondensed-Bold.ttf")]
[assembly: ExportFont("SairaExtraCondensed-SemiBold.ttf")]
[assembly: ExportFont("SairaExtraCondensed-Light.ttf")]
[assembly: ExportFont("SairaExtraCondensed-Medium.ttf")]


//SairaExtraCondensed-SemiBold
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace OWCE
{
    public partial class App : Application
    {
        public const string UnitDisplayUpdatedKey = "UnitDisplayUpdated";

        public static new App Current => Application.Current as App;
        public IOWBLE OWBLE { get; private set; }

        public string AppState { get; set; }

        // Just to print debug messages on the watch
        public string ReconnectingErrors { get; set; }
        public OWBoard CurrentBoard { get; set; }

        public DateTime TimeStarted = DateTime.Now;

#if DEBUG
        public const string OWCEApiServer = "api.dev.owce.app";
#else
        public const string OWCEApiServer = "api.owce.app";
#endif


        public static readonly BindableProperty MetricDisplayProperty = BindableProperty.Create(
            nameof(MetricDisplay),
            typeof(bool),
            typeof(App),
            false);

        public bool MetricDisplay
        {
            get { return (bool)GetValue(MetricDisplayProperty); }
            set { SetValue(MetricDisplayProperty, value); }
        }

        private string _boardConnectionCode = null;

        public string BoardConnectionCode
        {
            get { return _boardConnectionCode; }
        }

        private string _boardId = null;

        public string BoardId { get { return _boardId; } }

        public string LogsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "beta_ride_logs");

        public App()
        {
            AppState = "Unknown";
            ReconnectingErrors = "";

            IWatch watchService = DependencyService.Get<IWatch>();
            watchService.ListenForWatchMessages(CurrentBoard);
            WatchSyncEventHandler.ForceReconnect += HandleForceReconnect;

            MetricDisplay = Preferences.Get("metric_display", System.Globalization.RegionInfo.CurrentRegion.IsMetric);
            _boardConnectionCode = UserSecretsManager.Settings["BoardConnectionCode"];
            _boardId = UserSecretsManager.Settings["BoardId"];
            Debug.WriteLine("BoardConnectionCode: " + BoardConnectionCode);
            
            if (Directory.Exists(LogsDirectory) == false)
            {
                Directory.CreateDirectory(LogsDirectory);
            }

            InitializeComponent();

            AppState = "Initialized";
#if DEBUG
            // If simulator or emulator use MockOWBLE.
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                OWBLE = new MockOWBLE();
            }
            else
            {
                OWBLE = DependencyService.Get<IOWBLE>();
            }
#else
            OWBLE = DependencyService.Get<IOWBLE>();
#endif
            //MainPage = new MainFlyoutPage();
            MainPage = new NavigationPage(new BoardListPage());



            /*
            Debug.WriteLine("Before 1");
            Task.Run(async () =>
            {
                Debug.WriteLine("Before 2");
                await Task.Delay(1000);
                Debug.WriteLine("After 2");
            });
            Debug.WriteLine("After 1");
            */
            AppState = "PostMainPage";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start($"android={AppConstants.AppCenterAndroid};ios={AppConstants.AppCenteriOS}", typeof(Analytics), typeof(Crashes));

            AppState = "AppStarted";

            /*
            var cancellationTokenSource = new CancellationTokenSource();

            var file = Directory.GetFiles(App.Current.LogsDirectory, "*.bin").First();
            var rand = new Random();
            var baseBoard = new OWBaseBoard()
            {
                ID = "ow" + rand.Next(0, 999999).ToString("D6"),
                Name = Path.GetFileNameWithoutExtension(file),
                IsAvailable = true,
                NativePeripheral = file,
            };

           
            var board = await App.Current.ConnectToBoard(baseBoard, cancellationTokenSource.Token);
            if (board != null)
            {
                //MainPage = new NavigationPage(new TestPage());
                MainPage = new NavigationPage(new BoardPage(board)); // (new TestPage());
            }
            */
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            Console.WriteLine("App.xaml OnSleep");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            Console.WriteLine("App.xaml OnResume");

            // If we connected via watch, then show the BoardPage
            if (Current.CurrentBoard != null)
            {
                //MainPage.Navigation.PushModalAsync(new Xamarin.Forms.NavigationPage(new BoardPage(App.Current.CurrentBoard)));
            }

            var navigationStack = Current.MainPage.Navigation.NavigationStack;
            if (navigationStack.Count > 0)
            {
                var page = navigationStack.Last();
                Console.WriteLine("Current Page Type: " + page.GetType().Name);
            }
        }

        internal async Task<OWBoard> ConnectToBoard(OWBaseBoard baseBoard, CancellationToken token)
        {
            var didConnect = await OWBLE.Connect(baseBoard, token);
            if (didConnect)
            {
                return new OWBoard(OWBLE, baseBoard);
            }

            return null;
        }

        internal void DisconnectFromBoard()
        {
            /*
            OWBLE.Disconnect();
            OWBLE = null;
            */
        }

        private async void HandleForceReconnect()
        {
            Console.WriteLine("ForceReconnect Invoked");
            App.Current.ReconnectingErrors = "";
            try
            {
                try
                {
                    await App.Current.OWBLE.Disconnect();
                }
                catch (Exception ex)
                {
                    // Ignore this exception -- the board might not even be connected in the first place
                    Console.WriteLine("Already Disconnected: " + ex.Message);
                }

                OWBaseBoard baseBoard = App.Current.OWBLE.GetBoardFromUUID(App.Current.BoardId);
                var cancellationTokenSource = new CancellationTokenSource();

                var board = await App.Current.ConnectToBoard(baseBoard, cancellationTokenSource.Token);

                board.Init();
                _ = board.SubscribeToBLE();
                App.Current.CurrentBoard = board;
                App.Current.ReconnectingErrors = "Reconnect Success";
            }
            catch (Exception e)
            {
                Console.WriteLine("Error doing force reconnect: " + e.Message);
                Console.WriteLine("Stack Trace: " + e.StackTrace);
                App.Current.ReconnectingErrors = e.Message;
            }
        }

    }
}
