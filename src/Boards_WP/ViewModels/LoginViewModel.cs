using System;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Boards_WP.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUsersService _usersService;
        private readonly UserSession _userSession;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _errorMessage;

        public LoginViewModel(IUsersService usersService, UserSession userSession, INavigationService navigationService)
        {
            _usersService = usersService;
            _userSession = userSession;
            _navigationService = navigationService;
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
                var user = _usersService.Login(Email, password);

                if (user != null)
                {
                    _userSession.CurrentUser = user;
                    var mainVM = App.Services.GetRequiredService<MainViewModel>();
                    mainVM.IsLoggedIn = true;

                    if (((App)App.Current).m_window is MainWindow mainWindow)
                    {
                        _navigationService.Initialize(mainWindow.ContentFrame);
                        _navigationService.NavigateTo(typeof(Views.Pages.FeedView));
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