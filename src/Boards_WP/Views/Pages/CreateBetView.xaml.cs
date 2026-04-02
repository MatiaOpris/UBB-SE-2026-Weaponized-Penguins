using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreateBetView : Page
    {
        public CreateBetView()
        {
            this.InitializeComponent();
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                this.Frame.Navigate(typeof(BetsView));
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}