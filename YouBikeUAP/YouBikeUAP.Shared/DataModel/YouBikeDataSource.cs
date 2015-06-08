namespace YouBikeUAP.Data
{
  using System.Collections.ObjectModel;
  using System.Net.Http;
  using System.Threading.Tasks;
  using Common;
  using System;
  using System.Diagnostics;
  using Windows.Data.Json;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// YouBike 開放資料中的租賃站資料結構
  /// </summary>
  public class YouBikeDataStation
  {
    public string _id { get; }
    public string iid { get; }
    public string sv { get; }
    public string sd { get; }
    public string vtyp { get; }
    public string sno { get; }
    public string sna { get; }
    public string sip { get; }
    public string tot { get; }
    public string sbi { get; }
    public string sarea { get; }
    public string mday { get; }
    public double lat { get; }
    public double lng { get; }
    public string ar { get; }
    public string sareaen { get; }
    public string snaen { get; }
    public string aren { get; }
    public string nbcnt { get; }
    public string bemp { get; }
    public string act { get; }

    // 暫存根據參考點所計算出來的距離
    public double Distance { get; set; }
    // 較可讀的距離字串
    public string DistanceString { get; set; }

    public YouBikeDataStation(string _id, string iid, string sv, string sd, string vtyp, string sno, string sna,
                              string sip, string tot, string sbi, string sarea, string mday, double lat, double lng,
                              string ar, string sareaen, string snaen, string aren, string nbcnt, string bemp, string act)
    {
      this._id = _id;
      this.iid = iid;
      this.sv = sv;
      this.sd = sd;
      this.vtyp = vtyp;
      this.sno = sno;
      this.sna = sna;
      this.sip = sip;
      this.tot = tot;
      this.sbi = sbi;
      this.sarea = sarea;
      this.mday = mday;
      this.lat = lat;
      this.lng = lng;
      this.ar = ar;
      this.sareaen = sareaen;
      this.snaen = snaen;
      this.aren = aren;
      this.nbcnt = nbcnt;
      this.bemp = bemp;
      this.act = act;
    }

    public override string ToString() => sna;
  }

  /// <summary>
  /// YouBike 開放資料回傳的最上層資料結構
  /// </summary>
  public class YouBikeDataResult
  {
    public int offset { get; }
    public int limit { get; }
    public int count { get; }
    public string sort { get; }
    public ObservableCollection<YouBikeDataStation> Stations { get; }

    public YouBikeDataResult(int offset, int limit, int count, string sort)
    {
      this.offset = offset;
      this.limit = limit;
      this.count = count;
      this.sort = sort;
      Stations = new ObservableCollection<YouBikeDataStation>();
    }
  }

  /// <summary>
  /// 取得 YouBike 開放資料相關的資料運算
  /// </summary>
  public sealed class YouBikeDataSource
  {
    private static YouBikeDataSource _youbikeDataSource = new YouBikeDataSource();

    private YouBikeDataResult _result;
    public YouBikeDataResult Result => _result;

    private static double currentLat;
    private static double currentLng;

    /// <summary>
    /// 計算兩個站點根據目前參考點的距離排序依據
    /// </summary>
    /// <param name="a">站點 A</param>
    /// <param name="b">站點 B</param>
    /// <returns></returns>
    private static int NearStopComparison(YouBikeDataStation a, YouBikeDataStation b)
    {
      double diff = b.Distance - a.Distance;
      return diff > 0 ? -1 : diff == 0 ? 0 : 1;
    }

    /// <summary>
    /// 計算兩個經緯度座標在地球上的真實距離
    /// </summary>
    /// <param name="lat1">第一點的緯度座標</param>
    /// <param name="lng1">第一點的經度座標</param>
    /// <param name="lat2">第二點的緯度座標</param>
    /// <param name="lng2">第二點的經度座標</param>
    /// <returns>距離（單位: 公尺）</returns>
    private static double LatLngDistance(double lat1, double lng1, double lat2, double lng2)
    {
      const int R = 6371000;
      double dLat = (lat2 - lat1) * Math.PI / 180.0,
             dLng = (lng2 - lng1) * Math.PI / 180.0;
      double l1 = lat1 * Math.PI / 180.0,
             l2 = lat2 * Math.PI / 180.0;

      double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                 Math.Sin(dLng / 2.0) * Math.Sin(dLng / 2.0) * Math.Cos(l1) * Math.Cos(l2);
      double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      return R * c;
    }

    /// <summary>
    /// 根據參考點取出鄰近站點的資料
    /// </summary>
    /// <param name="lat">參考點的緯度</param>
    /// <param name="lng">參考點的經度</param>
    /// <returns>鄰近站點的 list</returns>
    public static async Task<List<YouBikeDataStation>> GetStationsByLocationAsync(double lat, double lng)
    {
      currentLat = lat;
      currentLng = lng;

      await _youbikeDataSource.GetYouBikeDataAsync(0, -1);

      List<YouBikeDataStation> stations = new List<YouBikeDataStation>();
      foreach (YouBikeDataStation station in _youbikeDataSource.Result.Stations)
      {
        station.Distance = LatLngDistance(lat, lng, station.lat, station.lng);
        station.DistanceString = string.Format("{0:0,0}", station.Distance);
        stations.Add(station);
      }
      // 根據距離排序
      stations.Sort(NearStopComparison);
      return stations.GetRange(0, Constants.NEAR_STATION_COUNT);
    }

    /// <summary>
    /// 根據租賃站代碼（sno）來取得站點資料
    /// </summary>
    /// <param name="sno">租賃站代碼</param>
    /// <returns>租賃站資料</returns>
    public static async Task<YouBikeDataStation> GetStationBySnoAsync(string sno)
    {
      await _youbikeDataSource.GetYouBikeDataAsync(0, -1);
      var matches = _youbikeDataSource.Result.Stations.Where((station) => station.sno.Equals(sno));
      if (matches.Count() == 1) return matches.First();
      return null;
    }

    /// <summary>
    /// 從 YouBike 開放資料平台讀取資料
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    private async Task GetYouBikeDataAsync(int offset, int limit)
    {
      if (_youbikeDataSource.Result != null &&
        _youbikeDataSource.Result.Stations.Count() > 0)
      {
        return;
      }

      // Initialize the http client the request object.
      var httpClient = new HttpClient();
      var requestUrl = Constants.YOUBIKE_DATA_URL;
      if (limit > -1)
      {
        requestUrl = string.Concat(requestUrl, string.Format("&offset={0}&limit={1}", offset, limit));
      }
      var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);

      string responseText = "";

      // send the request to get the station list json file.
      try
      {
        var response = await httpClient.SendAsync(httpRequest);
        responseText = await response.Content.ReadAsStringAsync();
      }
      catch (Exception e)
      {
        Debug.WriteLine("[YouBikeDataSouarce$GetYouBikeDataAsync] " + e.Message);
      }

      JsonObject jsonObject = JsonObject.Parse(responseText);
      JsonObject jsonResult = jsonObject["result"].GetObject();
      JsonArray jsonStations = jsonResult["results"].GetArray();

      _result = new YouBikeDataResult((int)jsonResult["offset"].GetNumber(),
                                      (int)jsonResult["limit"].GetNumber(),
                                      (int)jsonResult["count"].GetNumber(),
                                      jsonResult["sort"].GetString());

      foreach (JsonValue stationValue in jsonStations)
      {
        JsonObject station = stationValue.GetObject();
        _result.Stations.Add(new YouBikeDataStation(
          station["_id"].GetString(),
          station["iid"].GetString(),
          station["sv"].GetString(),
          station["sd"].GetString(),
          station["vtyp"].GetString(),
          station["sno"].GetString(),
          station["sna"].GetString(),
          station["sip"].GetString(),
          station["tot"].GetString(),
          station["sbi"].GetString(),
          station["sarea"].GetString(),
          station["mday"].GetString(),
          double.Parse(station["lat"].GetString()),
          double.Parse(station["lng"].GetString()),
          station["ar"].GetString(),
          station["sareaen"].GetString(),
          station["snaen"].GetString(),
          station["aren"].GetString(),
          station["nbcnt"].GetString(),
          station["bemp"].GetString(),
          station["act"].GetString()
        ));
      }

    }
  }
}
