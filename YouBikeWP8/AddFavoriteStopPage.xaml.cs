namespace YouBikeWP8
{
  using System;
  using System.Collections.ObjectModel;
  using System.Windows;
  using System.Windows.Controls;
  using Microsoft.Phone.Controls;
  using ViewModels;

  public partial class AddFavoriteStopPage : PhoneApplicationPage
  {
    string[] autoCompletes;
    ObservableCollection<BikeStopViewModel> ResultItems = new ObservableCollection<BikeStopViewModel>();

    public AddFavoriteStopPage()
    {
      InitializeComponent();

      InitData();

      AddBox.ItemsSource = autoCompletes;
    }

    private async void InitData()
    {
      await App.ViewModel.LoadData();
      int len = App.ViewModel.Items.Count;

      autoCompletes = new string[len];

      for (int i = 0; i < len; ++i)
      {
        autoCompletes[i] = App.ViewModel.Items[i].Name;
      }

      SearchResultList.ItemsSource = ResultItems;
    }

    private void OnTextChanged(object sender, RoutedEventArgs e)
    {
      string text = AddBox.Text;
      int length = autoCompletes.Length;
      ResultItems.Clear();

      for (int i = 0; i < length; ++i)
      {
        if (autoCompletes[i].Contains(text))
        {
          ResultItems.Add(App.ViewModel.Items[i]);
        }
      }

    }

    private void OnSearchResultItemSelected(object sender, SelectionChangedEventArgs e)
    {
      BikeStopViewModel stop = (BikeStopViewModel)SearchResultList.SelectedItem;
      NavigationService.Navigate(new Uri("/BikeStopPage.xaml?si=" + App.ViewModel.Indecies.IndexOf(stop.Name), UriKind.Relative));
    }

  }
}