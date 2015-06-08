using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YouBikeUAP.Data;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkID=390556

namespace YouBikeUAP
{
  /// <summary>
  /// 可以在本身使用或巡覽至框架內的空白頁面。
  /// </summary>
  public sealed partial class StationPage : Page
  {
    public StationPage()
    {
      this.InitializeComponent();
    }

    /// <summary>
    /// 在此頁面即將顯示在框架中時叫用。
    /// </summary>
    /// <param name="e">描述如何到達此頁面的事件資料。
    /// 這個參數通常用來設定頁面。</param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      var station = (YouBikeDataStation)e.Parameter;
      // update page info
      StationName.Text = station.sna;
      BikeStopAvailable.Text = station.sbi;
      BikeStopCapacity.Text = station.bemp;

      var geopos = new BasicGeoposition();
      geopos.Latitude = station.lat;
      geopos.Longitude = station.lng;

      myMapControl.Center = new Geopoint(geopos);
      myMapControl.ZoomLevel = 15;

      base.OnNavigatedTo(e);
    }
  }
}
