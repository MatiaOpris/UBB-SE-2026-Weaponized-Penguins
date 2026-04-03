using Boards_WP.Data.Models;
using Boards_WP.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Boards_WP.Views.Pages
{
    public sealed partial class PlaceBetView : Page
    {
        public PlaceBetViewModel ViewModel { get; }

        public PlaceBetView()
        {
            ViewModel = App.GetService<PlaceBetViewModel>();
            this.InitializeComponent();
            this.DataContext = ViewModel;

            ViewModel.BetPlaced += OnBetPlaced;
            ViewModel.BetCancelled += OnBetCancelled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is BetPlacementNavigationData payload)
            {
                ViewModel.Initialize(payload);
            }
        }

        private async void OnBetPlaced()
        {
            LuckPopup.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(1000);
            LuckPopup.Visibility = Visibility.Collapsed;

            NavigateBack();
        }

        private void OnBetCancelled()
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(BetsView));
            }
        }
    }
}
