namespace YouBikeWP8.ViewModels
{
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Net;
  using System.Threading.Tasks;

  public class MainViewModel : INotifyPropertyChanged
  {
    public MainViewModel()
    {
      this.Items = new ObservableCollection<BikeStopViewModel>();
      Indecies = new List<string>();
    }

    public ObservableCollection<BikeStopViewModel> Items { get; private set; }
    public List<string> Indecies { get; private set; }

    const int SEARCH_RADIUS = 5;

    public bool IsDataLoaded
    {
      get;
      private set;
    }

    public async Task LoadData()
    {
      if (Items.Count > 0)
      {
        return;
      }

      var respText = await LoadStops();
      Debug.WriteLine(respText);
      JObject respObj = JObject.Parse(respText);
      JObject ybResult = (JObject) respObj["result"];
      JArray stations = (JArray) ybResult["results"];

      int i = 0;
      foreach (JObject station in stations)
      {
        Items.Add(new BikeStopViewModel() {
          Id = (string) station["sno"],
          Name = (string) station["sna"],
          NameEn = (string) station["snaen"],
          District = (string) station["sarea"],
          Address = (string) station["ar"],
          Latitude = double.Parse((string) station["lat"]),
          Longitude = double.Parse((string) station["lng"]),
          Capacity = int.Parse((string) station["sbi"]),
          Availability = int.Parse((string) station["bemp"])
        });
        i++;
        /*
        if (i > 64)
        {
          break;
        }
        */
      }

      foreach (BikeStopViewModel stop in Items)
      {
        Indecies.Add(stop.Name);
      }

      this.IsDataLoaded = true;
    }

    private Task<string> LoadStops()
    {
      var client = new WebClient();
      var tcs = new TaskCompletionSource<string>();

      try
      {
        client.DownloadStringCompleted += (s, e) =>
        {
          if (e.Error == null)
          {
            tcs.TrySetResult(e.Result);
          }
          else
          {
            tcs.TrySetException(e.Error);
          }
        };
        client.DownloadStringAsync(new Uri(Constants.YOUBIKE_DATA_URL, UriKind.Absolute));
      }
      catch (Exception ex)
      {
        tcs.TrySetException(ex);
      }

      return tcs.Task;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String propertyName)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (null != handler)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}
