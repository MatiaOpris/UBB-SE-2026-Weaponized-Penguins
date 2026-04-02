using Microsoft.UI.Xaml.Controls;

using Boards_WP.ViewModels;

using CommunityToolkit.Mvvm.Input;

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
    }
}