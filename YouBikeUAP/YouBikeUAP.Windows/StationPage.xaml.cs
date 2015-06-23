namespace YouBikeUAP
{
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;
  using Common;
  using Data;
  using System.Diagnostics;
  using Bing.Maps;
  using Microsoft.ApplicationInsights;
  using Windows.UI.Xaml;



  /// <summary>
  /// 可以在本身使用或巡覽至框架內的空白頁面。
  /// </summary>
  public sealed partial class StationPage : Page
  {
    public string MapsToken { get; private set; }
    public double Longitude { get; private set; }
    public double Latitude { get; private set; }

    public int Availability { get; private set; }

    public int Capacity { get; private set; }

    public string Title { get; set; }

    public YouBikeDataStation Station { get; private set; }

    private TelemetryClient telemetryClient;

    private readonly NavigationHelper navigationHelper;

    /// <summary>
    /// 取得 NavigationHelper，以用來協助巡覽及處理序生命週期管理。
    /// </summary>
    public NavigationHelper NavigationHelper
    {
      get { return this.navigationHelper; }
    }

    public StationPage()
    {
      DataContext = this;
      MapsToken = Constants.WIN81_BING_MAPS_KEY;

      telemetryClient = new TelemetryClient();

      this.InitializeComponent();


      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
      this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
    }

    private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      Station = (YouBikeDataStation)e.NavigationParameter;

      stationMap.Center = new Location(Station.lat, Station.lng);

      Title = Station.sna;
      Availability =  int.Parse(Station.sbi);
      Capacity = int.Parse(Station.bemp);

      Pushpin stationPin = new Pushpin();
      stationPin.Text = Station.sna;
      MapLayer.SetPosition(stationPin, new Location(Station.lat, Station.lng));
      stationMap.Children.Add(stationPin);
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
