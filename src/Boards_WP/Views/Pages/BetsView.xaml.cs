using Boards_WP.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Boards_WP.Views.Pages
{
    public sealed partial class BetsView : Page
    {
        public BetsViewModel ViewModel { get; }

        public BetsView()
        {
            ViewModel = App.GetService<BetsViewModel>();
            ViewModel.CreateBetCommand = new RelayCommand(NavigateToCreateBet);
            this.InitializeComponent();
        }

        private void NavigateToCreateBet()
        {
            this.Frame.Navigate(typeof(CreateBetView));
        }

        public Visibility BooleanToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility BooleanToInverseVisibility(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var keywords = e.Parameter as string;
            ViewModel.Initialize(keywords);
        }
    }
}