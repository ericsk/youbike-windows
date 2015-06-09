namespace YouBikeUAP
{
  using System;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using Windows.Graphics.Display;
  using Windows.UI.ViewManagement;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;
  using Windows.Devices.Geolocation;
  using Windows.UI.Xaml;
  using Common;
  using Data;
  using System.Threading.Tasks;

  public sealed partial class MainPage : Page
  {
    private readonly NavigationHelper navigationHelper;

    public ObservableCollection<YouBikeDataStation> Stations { get; private set; }

    private StatusBar statusBar;
    private StatusBarProgressIndicator progressIndicator;

    public MainPage()
    {
      this.InitializeComponent();

      // Use the shared, customized view model as the data context.
      DataContext = this;

      Stations = new ObservableCollection<YouBikeDataStation>();

      // 只有直式方向才支援中樞
      DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

      this.NavigationCacheMode = NavigationCacheMode.Required;

      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
      this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

      // get the statusbar 
      statusBar = StatusBar.GetForCurrentView();
      progressIndicator = statusBar.ProgressIndicator;
    }

    public NavigationHelper NavigationHelper
    {
      get { return this.navigationHelper; }
    }

    private void NavigationHelper_LoadState(object sender, LoadStateEventArgs args)
    {
      LoadNearStopData();
    }

    private async void LoadNearStopData()
    {
      // show the progress indicator
      await statusBar.ShowAsync();
      await progressIndicator.ShowAsync();

      // retrive the user's location
      var geolocator = new Geolocator();
      geolocator.DesiredAccuracyInMeters = 50;

      Geoposition geopos = null;
      try
      {
        geopos = await geolocator.GetGeopositionAsync(
             maximumAge: TimeSpan.FromMinutes(5),
             timeout: TimeSpan.FromSeconds(10)
            );
      }
      catch (Exception e)
      {
        Debug.WriteLine("[HubPage$LoadState] " + e.Message);
      }

      if (geopos != null)
      {
        var pos = geopos.Coordinate.Point.Position;
        var stations = await YouBikeDataSource.GetStationsByLocationAsync(pos.Latitude, pos.Longitude);
        Stations.Clear();
        foreach (var station in stations)
        {
          Stations.Add(station);
        }
      }

      await progressIndicator.HideAsync();
      await statusBar.HideAsync();
    }

    private void OnRefreshCommandClick(object sender, RoutedEventArgs e)
    {
      LoadNearStopData();
    }


    private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }
    

    private void OnNearStationItemClick(object sender, ItemClickEventArgs e)
    {
      if (!Frame.Navigate(typeof(StationPage), e.ClickedItem))
      {
        throw new Exception("切換頁面發生錯誤");
      }
    }

    #region NavigationHelper Registration

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedFrom(e);
    }

    #endregion

  }
}
