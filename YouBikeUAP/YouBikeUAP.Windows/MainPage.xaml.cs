namespace YouBikeUAP
{
  using System;
  using System.Diagnostics;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;
  using Windows.Devices.Geolocation;
  using Microsoft.ApplicationInsights;
  using Common;
  using Data;
  using Microsoft.ApplicationInsights.DataContracts;
  using System.Collections.Generic;

  public sealed partial class MainPage : Page
  {
    private NavigationHelper navigationHelper;
    public ObservableCollection<YouBikeDataStation> Stations { get; private set; }

    private TelemetryClient telemetryClient;

    /// <summary>
    /// 取得 NavigationHelper，以用來協助巡覽及處理序生命週期管理。
    /// </summary>
    public NavigationHelper NavigationHelper
    {
      get { return this.navigationHelper; }
    }

    public MainPage()
    {
      telemetryClient = new TelemetryClient();

      this.InitializeComponent();

      DataContext = this;

      Stations = new ObservableCollection<YouBikeDataStation>();

      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
    }

    private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      await LoadNearStopData();
    }

    private async Task LoadNearStopData()
    {
      progressRing.IsActive = true;

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
        telemetryClient.TrackException(e);
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

        var dict = new Dictionary<string, string>();
        dict.Add("Latitude", pos.Latitude.ToString());
        dict.Add("Longitude", pos.Longitude.ToString());
        telemetryClient.TrackEvent("UserLocation", dict);
      }
      else
      {
        telemetryClient.TrackEvent("Unknown Location");
      }

      progressRing.IsActive = false;
    }


    #region NavigationHelper 註冊

    /// <summary>
    /// 本區段中提供的方法只用來允許
    /// NavigationHelper 可回應頁面的巡覽方法。
    /// 頁面專屬邏輯應該放在事件處理常式中
    /// <see cref="Common.NavigationHelper.LoadState"/>
    /// 和 <see cref="Common.NavigationHelper.SaveState"/> 的事件處理常式中。
    /// 巡覽參數可用於 LoadState 方法
    /// 除了先前的工作階段期間保留的頁面狀態。
    /// </summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      this.navigationHelper.OnNavigatedFrom(e);
    }

    #endregion

    private void OnStationItemClick(object sender, ItemClickEventArgs args)
    {
      var station = (YouBikeDataStation)args.ClickedItem;
      var dict = new Dictionary<string, string>();
      dict.Add("SiteName", station.sna);
      telemetryClient.TrackEvent("StationClick", dict);

      if (!Frame.Navigate(typeof(StationPage), args.ClickedItem))
      {
        var e = new Exception("切換頁面發生錯誤");
        telemetryClient.TrackException(e);
        throw e;
      }
    }
  }
}
