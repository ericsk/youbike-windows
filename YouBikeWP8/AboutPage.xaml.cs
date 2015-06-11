namespace YouBikeWP8
{
  using System.Windows;
  using Microsoft.Phone.Controls;
  using Microsoft.Phone.Tasks;

  public partial class AboutPage : PhoneApplicationPage
  {
    public AboutPage()
    {
      InitializeComponent();
    }

    private void OnRatingClicked(object sender, RoutedEventArgs e)
    {
      MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
      reviewTask.Show();
    }
  }
}