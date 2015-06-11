namespace YouBikeWP8
{
  using Microsoft.Phone.Controls;
  using Microsoft.Phone.Maps;
  using Microsoft.Phone.Maps.Controls;
  using Microsoft.Phone.Maps.Toolkit;
  using Microsoft.Phone.Shell;
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Device.Location;
  using System.Diagnostics;
  using System.IO.IsolatedStorage;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Windows;
  using System.Windows.Navigation;
  using Resources;
  using ViewModels;
  using Windows.Devices.Geolocation;

  public class BikeStopPinModel : INotifyPropertyChanged
  {
    private string _name;
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        if (value != _name)
        {
          _name = value;
          NotifyPropertyChanged("Name");
        }
      }
    }

    private GeoCoordinate _coordinate;

    [TypeConverter(typeof(GeoCoordinateConverter))]
    public GeoCoordinate Coordinate
    {
      get
      {
        return _coordinate;
      }
      set
      {
        if (_coordinate != value)
        {
          _coordinate = value;
          NotifyPropertyChanged("Coordinate");
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (null != handler)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }

  public partial class MapsViewPage : PhoneApplicationPage
  {
    ProgressIndicator progressIndicator = new ProgressIndicator()
    {
      IsVisible = true,
      IsIndeterminate = true,
      Text = AppResources.Locating
    };
    IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
    ObservableCollection<BikeStopPinModel> BikeStops = new ObservableCollection<BikeStopPinModel>();

    private Geolocator geolocator;

    public MapsViewPage()
    {
      InitializeComponent();
      MapExtensionsSetup(MapsView);

      geolocator = new Geolocator() { MovementThreshold = 10, DesiredAccuracyInMeters = 5000 };
      StopsControl.ItemsSource = BikeStops;

      SystemTray.SetProgressIndicator(this, progressIndicator);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      Tracking();

      BuildLocalizedApplicationBar();
    }

    private async void Tracking()
    {
      // find your location
      SystemTray.SetIsVisible(this, true);
      progressIndicator.IsVisible = true;

      try
      {
        if (!appSettings.Contains(Constants.TRACKING) || (bool)appSettings[Constants.TRACKING])
        {
          Geoposition position = await geolocator.GetGeopositionAsync(
                  maximumAge: TimeSpan.FromMinutes(5),
                  timeout: TimeSpan.FromSeconds(10)
              );
          GeoCoordinate myLocation = position.Coordinate.ToGeoCoordinate();


          MyLocationMarker.GeoCoordinate = myLocation;
          MyLocationMarker.Visibility = Visibility.Visible;

          MapsView.Center = myLocation;
        }
        else
        {
          MapsView.Center = new GeoCoordinate(25.0380, 121.5487);
        }

        ObservableCollection<BikeStopViewModel> NearList = (ObservableCollection<BikeStopViewModel>)appSettings["NearListTemp"];
        foreach (BikeStopViewModel b in NearList)
        {
          BikeStops.Add(new BikeStopPinModel()
          {
            Name = String.Format("{0}", b.Name),
            Coordinate = new GeoCoordinate(b.Latitude, b.Longitude)
          });
        }

      }
      catch (Exception ex)
      {
        // TODO
        Debug.WriteLine("[MapsViewPage][Locating] " + ex.Message);
      }
      SystemTray.SetIsVisible(this, false);
      progressIndicator.IsVisible = false;
    }

    private void BuildLocalizedApplicationBar()
    {
      ApplicationBar = new ApplicationBar();

      ApplicationBarIconButton trackMeButton =
          new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.location.circle.png", UriKind.Relative));
      trackMeButton.Text = AppResources.AppBarMyLocation;
      trackMeButton.Click += (s, e) =>
      {
        Tracking();
      };

      ApplicationBar.Buttons.Add(trackMeButton);
      ApplicationBar.Mode = ApplicationBarMode.Minimized;
    }

    #region Helpers

    /// <summary>
    /// Setup the map extensions objects.
    /// All named objects inside the map extensions will have its references properly set
    /// </summary>
    /// <param name="map">The map that uses the map extensions</param>
    private void MapExtensionsSetup(Map map)
    {
      ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(map);
      var runtimeFields = GetType().GetRuntimeFields();

      foreach (DependencyObject i in children)
      {
        var info = i.GetType().GetProperty("Name");

        if (info != null)
        {
          string name = (string)info.GetValue(i);

          if (name != null)
          {
            foreach (FieldInfo j in runtimeFields)
            {
              if (j.Name == name)
              {
                j.SetValue(this, i);
                break;
              }
            }
          }
        }
      }
    }

    #endregion

    private void OnMapsLoaded(object sender, RoutedEventArgs e)
    {
      MapsSettings.ApplicationContext.ApplicationId = Constants.MAPS_APPLICATION_ID;
      MapsSettings.ApplicationContext.AuthenticationToken = Constants.MAPS_AUTHENTICATION_TOKEN;
    }
  }
}