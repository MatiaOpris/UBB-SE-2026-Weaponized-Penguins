using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views.Pages
{
    public sealed partial class LoginView : Page
    {
        public LoginViewModel ViewModel { get; }

        public Visibility StringToVis(string error) => string.IsNullOrEmpty(error) ? Visibility.Collapsed : Visibility.Visible;

        public LoginView()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<LoginViewModel>();
        }

    }
}