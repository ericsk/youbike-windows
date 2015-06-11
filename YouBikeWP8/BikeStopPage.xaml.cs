namespace YouBikeWP8
{
  using Microsoft.Phone.Controls;
  using Microsoft.Phone.Maps;
  using Microsoft.Phone.Maps.Controls;
  using Microsoft.Phone.Maps.Toolkit;
  using Microsoft.Phone.Shell;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Device.Location;
  using System.IO.IsolatedStorage;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Windows;
  using System.Windows.Navigation;
  using Resources;
  using ViewModels;
  using Windows.Devices.Geolocation;

  public partial class BikeStopPage : PhoneApplicationPage
  {
    ProgressIndicator progressIndicator = new ProgressIndicator()
    {
      IsVisible = true,
      IsIndeterminate = true
    };

    private string StopId;
    private int ListId;
    private IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

    ApplicationBarIconButton refreshButton;

    public BikeStopPage()
    {
      InitializeComponent();

      SystemTray.SetProgressIndicator(this, progressIndicator);

      MapExtensionsSetup(BikeStopMap);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      String id;

      if (NavigationContext.QueryString.TryGetValue("si", out id))
      {
        ListId = int.Parse(id);
        BikeStopViewModel stop = App.ViewModel.Items[ListId];
        StopId = stop.Id;
        BikeStopName.Text = stop.Name;
        BikeStopMap.Center = new GeoCoordinate(stop.Latitude, stop.Longitude);
        BikeStopDistrict.Text = stop.District;
        BikeStopAddress.Text = stop.Address;
        BikeStopAvailable.Text = stop.Availability.ToString();
        BikeStopCapacity.Text = stop.Capacity.ToString();

        TargetMarker.Content = stop.Name;
        TargetMarker.GeoCoordinate = new GeoCoordinate(stop.Latitude, stop.Longitude);
        TargetMarker.Visibility = Visibility.Visible;

        BuildLocalizedApplicationBar();

        TrackPosition();
      }
    }

    private async void TrackPosition()
    {
      refreshButton.IsEnabled = false;
      SystemTray.SetIsVisible(this, true);
      progressIndicator.Text = AppResources.Loading;
      progressIndicator.IsVisible = true;

      try
      {
        if (!appSettings.Contains(Constants.TRACKING) || (bool)appSettings[Constants.TRACKING])
        {
          Geolocator geolocator = new Geolocator() { MovementThreshold = 10, DesiredAccuracyInMeters = 5000 };
          Geoposition pos = await geolocator.GetGeopositionAsync(
              maximumAge: TimeSpan.FromMinutes(5),
              timeout: TimeSpan.FromSeconds(10));
          YourPosition.GeoCoordinate = new GeoCoordinate(pos.Coordinate.Latitude, pos.Coordinate.Longitude);
          YourPosition.Visibility = Visibility.Visible;
        }
      }
      catch (Exception)
      {
        MessageBox.Show(AppResources.NetworkError, AppResources.NetworkErrorCaption, MessageBoxButton.OK);
      }

      progressIndicator.IsVisible = false;
      SystemTray.SetIsVisible(this, false);
      refreshButton.IsEnabled = true;
    }

    private void BuildLocalizedApplicationBar()
    {
      ApplicationBar = new ApplicationBar();

      bool isFavorited = false;
      List<int> favorites;
      if (appSettings.Contains(Constants.FAVORITED_LIST))
      {
        favorites = (List<int>)appSettings[Constants.FAVORITED_LIST];
        if (favorites.IndexOf(ListId) != -1)
        {
          isFavorited = true;
        }
      }

      ApplicationBarIconButton favoriteButton =
          new ApplicationBarIconButton(new Uri(isFavorited ? "/Assets/AppBar/appbar.star.minus.png" : "/Assets/AppBar/appbar.star.add.png", UriKind.Relative));
      favoriteButton.Text = isFavorited ? AppResources.BikeStopUnfavoriteButton : AppResources.BikeStopFavoriteButton;
      favoriteButton.Click += (s, e) =>
      {
        List<int> favs;

        if (isFavorited)
        {
          // unfavor
          favs = (List<int>)appSettings[Constants.FAVORITED_LIST];
          int index = favs.IndexOf(ListId);
          if (index != -1)
          {
            favs.RemoveAt(index);
          }
          favoriteButton.IconUri = new Uri("/Assets/AppBar/appbar.star.add.png", UriKind.Relative);
          favoriteButton.Text = AppResources.BikeStopFavoriteButton;
          isFavorited = false;

          MessageBox.Show(AppResources.FavoriteRemoved);
        }
        else
        {
          // fav
          if (appSettings.Contains(Constants.FAVORITED_LIST))
          {
            favs = (List<int>)appSettings[Constants.FAVORITED_LIST];
          }
          else
          {
            favs = new List<int>();
          }
          favs.Add(ListId);
          favoriteButton.IconUri = new Uri("/Assets/AppBar/appbar.star.minus.png", UriKind.Relative);
          favoriteButton.Text = AppResources.BikeStopUnfavoriteButton;
          isFavorited = true;

          MessageBox.Show(AppResources.FavoriteAdded);

        }
        appSettings[Constants.FAVORITED_LIST] = favs;
        appSettings.Save();
      };

      refreshButton =
          new ApplicationBarIconButton(new Uri("/Assets/AppBar/sync.png", UriKind.Relative))
          {
            Text = AppResources.BikeStopRefreshButton
          };
      refreshButton.Click += (s, e) =>
      {
        TrackPosition();
      };

      ApplicationBar.Buttons.Add(favoriteButton);
      ApplicationBar.Buttons.Add(refreshButton);

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