namespace YouBikeUAP
{
  using System;
  using System.Diagnostics;
  using Windows.Devices.Geolocation;
  using Windows.Services.Maps;
  using Windows.UI;
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
      MapService.ServiceToken = BingMapsKey;

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
      Address.Text = station.ar;

      var geopos = new Geopoint(new BasicGeoposition() { Latitude = station.lat, Longitude = station.lng });

      stationMapControl.Center = geopos;
      stationMapControl.ZoomLevel = 15;

      // add map icon
      MapIcon stationIcon = new MapIcon();
      stationIcon.Location = geopos;
      stationIcon.Title = station.sna;
      stationMapControl.MapElements.Add(stationIcon);

    }

    private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    private async void OnDirectionsCommandClick(object sender, RoutedEventArgs args)
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
        var startPoint = geopos.Coordinate.Point;
        var endPoint = new Geopoint(new BasicGeoposition() { Latitude = station.lat, Longitude = station.lng });

        MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetWalkingRouteAsync(startPoint, endPoint);

        if (routeResult.Status == MapRouteFinderStatus.Success)
        {
          // Use the route to initialize a MapRouteView.
          MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
          viewOfRoute.RouteColor = Colors.Yellow;
          viewOfRoute.OutlineColor = Colors.Black;

          // Add the new MapRouteView to the Routes collection
          // of the MapControl.
          stationMapControl.Routes.Add(viewOfRoute);

          // Fit the MapControl to the route.
          await stationMapControl.TrySetViewBoundsAsync(
              routeResult.Route.BoundingBox,
              null,
              MapAnimationKind.None);
        }
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
