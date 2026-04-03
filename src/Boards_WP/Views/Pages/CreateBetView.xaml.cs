using Boards_WP.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreateBetView : Page
    {
        public CreateBetViewModel ViewModel { get; }

        public CreateBetView()
        {
            ViewModel = App.GetService<CreateBetViewModel>();
            this.InitializeComponent();
            this.DataContext = ViewModel;
            ViewModel.BetCreated += OnBetCreated;
            ViewModel.BetCancelled += OnBetCancelled;
        }

        private void OnBetCancelled()
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void OnBetCreated()
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
            else
                this.Frame.Navigate(typeof(BetsView));
        }

        private void Comment_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.IsPost = false;
        }
    }
}