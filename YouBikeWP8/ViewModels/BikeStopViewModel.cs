using System;
using System.ComponentModel;

namespace YouBikeWP8.ViewModels
{
  public class BikeStopViewModel : INotifyPropertyChanged
  {
    private string _id;
    public string Id
    {
      get
      {
        return _id;
      }
      set
      {
        if (value != _id)
        {
          _id = value;
          NotifyPropertyChanged("Id");
        }
      }
    }

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

    private string _nameEn;
    public string NameEn
    {
      get
      {
        return _nameEn;
      }
      set
      {
        if (value != _nameEn)
        {
          _nameEn = value;
          NotifyPropertyChanged("NameEn");
        }
      }
    }

    private string _district;
    public string District
    {
      get
      {
        return _district;
      }
      set
      {
        if (value != _district)
        {
          _district = value;
          NotifyPropertyChanged("District");
        }
      }
    }

    private string _address;
    public string Address
    {
      get
      {
        return _address;
      }
      set
      {
        if (value != _address)
        {
          _address = value;
          NotifyPropertyChanged("Address");
        }
      }
    }

    private int _availability;
    public int Availability
    {
      get
      {
        return _availability;
      }
      set
      {
        if (value != _availability)
        {
          _availability = value;
          NotifyPropertyChanged("Availability");
        }
      }
    }

    private int _capacity;
    public int Capacity
    {
      get
      {
        return _capacity;
      }
      set
      {
        if (value != _capacity)
        {
          _capacity = value;
          NotifyPropertyChanged("Capacity");
        }
      }
    }

    private double _latitude;
    public double Latitude
    {
      get
      {
        return _latitude;
      }
      set
      {
        if (value != _latitude)
        {
          _latitude = value;
          NotifyPropertyChanged("Latitude");
        }
      }
    }

    private double _longitude;
    public double Longitude
    {
      get
      {
        return _longitude;
      }
      set
      {
        if (value != _longitude)
        {
          _longitude = value;
          NotifyPropertyChanged("Longitude");
        }
      }
    }

    private string _distance;
    public string Distance
    {
      get
      {
        return _distance;
      }
      set
      {
        if (value != _distance)
        {
          _distance = value;
          NotifyPropertyChanged("Distance");
        }
      }
    }

    private string _iconType;
    public string IconType
    {
      get
      {
        return _iconType;
      }
      set
      {
        if (value != _iconType)
        {
          _iconType = value;
          NotifyPropertyChanged("IconType");
        }
      }
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

