using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Boards_WP.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUsersService usersService;
        private readonly UserSession userSession;
        private readonly INavigationService navigationService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string errorMessage;

        public LoginViewModel(IUsersService usersService, UserSession userSession, INavigationService navigationService)
        {
            this.usersService = usersService;
            this.userSession = userSession;
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void Login(object parameter)
        {
            string password = string.Empty;
            if (parameter is Microsoft.UI.Xaml.Controls.PasswordBox passwordBox)
            {
                password = passwordBox.Password;
            }

            if (string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Password is required.";
                return;
            }

            try
            {
                var user = usersService.Login(Email, password);

                if (user != null)
                {
                    userSession.CurrentUser = user;
                    var mainVM = App.Services.GetRequiredService<MainViewModel>();
                    mainVM.IsLoggedIn = true;

                    if (((App)App.Current).M_window is MainWindow mainWindow)
                    {
                        navigationService.Initialize(mainWindow.ContentFrame);
                        navigationService.NavigateTo(typeof(Views.Pages.FeedView));
                        mainWindow.LoginFrame.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}