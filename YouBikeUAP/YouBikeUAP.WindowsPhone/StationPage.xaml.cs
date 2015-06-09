namespace YouBikeUAP
{
  using System;
  using System.Diagnostics;
  using Windows.Devices.Geolocation;
  using Windows.UI.ViewManagement;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Controls.Maps;
  using Windows.UI.Xaml.Navigation;
  using YouBikeUAP.Common;
  using YouBikeUAP.Data;

  public sealed partial class StationPage : Page
  {
    private readonly NavigationHelper navigationHelper;

    public string BingMapsKey { get; private set; }

    private YouBikeDataStation station;

    private StatusBar statusBar;
    private StatusBarProgressIndicator progressIndicator;

    public StationPage()
    {
      this.InitializeComponent();

      DataContext = this;
      BingMapsKey = Constants.BING_MAPS_KEY;

      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
      this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

      // get the statusbar 
      statusBar = StatusBar.GetForCurrentView();
      progressIndicator = statusBar.ProgressIndicator;
    }

    private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      station = (YouBikeDataStation)e.NavigationParameter;
      // update page info
      StationName.Text = station.sna;
      BikeStopAvailable.Text = station.sbi;
      BikeStopCapacity.Text = station.bemp;

      var geopos = new Geopoint(new BasicGeoposition() { Latitude = station.lat, Longitude = station.lng });

      stationMapControl.Center = geopos;
      stationMapControl.ZoomLevel = 15;

      // add map icon
      MapIcon stationIcon = new MapIcon();
      stationIcon.Location = geopos;
      stationIcon.Title = station.sna + "站";
      stationMapControl.MapElements.Add(stationIcon);

    }

    private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    private async void OnDirectionsCommandClick(object sender, RoutedEventArgs e)
    {
      await Windows.System.Launcher.LaunchUriAsync
      (
          new Uri(string.Format("ms-walk-to:?destination.latitude={0}&destination.longitude={1}&destination.name={2}",
            station.lat, station.lng, station.sna), UriKind.Absolute)
      );
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
