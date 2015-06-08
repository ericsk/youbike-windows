namespace YouBikeUAP
{
  using Windows.Devices.Geolocation;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;
  using YouBikeUAP.Common;
  using YouBikeUAP.Data;

  public sealed partial class StationPage : Page
  {
    private readonly NavigationHelper navigationHelper;

    public string BingMapsKey { get; private set; }

    public StationPage()
    {
      this.InitializeComponent();

      DataContext = this;
      BingMapsKey = Constants.BING_MAPS_KEY;

      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
      this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
    }

    private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      var station = (YouBikeDataStation)e.NavigationParameter;
      // update page info
      StationName.Text = station.sna;
      BikeStopAvailable.Text = station.sbi;
      BikeStopCapacity.Text = station.bemp;

      var geopos = new BasicGeoposition();
      geopos.Latitude = station.lat;
      geopos.Longitude = station.lng;

      myMapControl.Center = new Geopoint(geopos);
      myMapControl.ZoomLevel = 15;
    }

    private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
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
