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
  using Data;

  public sealed partial class MainPage : Page
  {
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

      // get the statusbar 
      statusBar = StatusBar.GetForCurrentView();
      progressIndicator = statusBar.ProgressIndicator;

      // 頁面載入後的事件
      Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs args)
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
        foreach (var station in stations)
        {
          Stations.Add(station);
        }
      }

      await progressIndicator.HideAsync();
      await statusBar.HideAsync();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);
    }

    private void OnNearStationItemClick(object sender, ItemClickEventArgs e)
    {
      if (!Frame.Navigate(typeof(StationPage), e.ClickedItem))
      {
        throw new Exception("切換頁面發生錯誤");
      }
    }
  }
}
