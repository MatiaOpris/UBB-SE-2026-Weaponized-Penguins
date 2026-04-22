using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Boards_WP
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel;
        public MainViewModel MainViewModel => mainViewModel;

        public Visibility BoolToVis(bool isLoggedIn) => isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

        public MainWindow()
        {
            mainViewModel = App.Services.GetRequiredService<MainViewModel>();
            mainViewModel.IsLoggedIn = false;

            this.InitializeComponent();

            if (App.Current is App myApp)
            {
                myApp.M_window = this;
            }
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.Initialize(LoginFrame);
            navigationService.NavigateTo(typeof(Views.Pages.LoginView));
        }

        public void NavigateToPage(Type pageType, object parameter = null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }
    }
}