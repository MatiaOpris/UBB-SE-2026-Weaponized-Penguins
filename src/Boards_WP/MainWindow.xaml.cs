using System;

using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Boards_WP
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;
        public MainViewModel MainViewModel => _mainViewModel;

        public MainWindow()
        {
            this.InitializeComponent();

            _mainViewModel = App.Services.GetRequiredService<MainViewModel>();

            this.InitializeComponent();

            if (App.Current is App myApp)
            {
                myApp.m_window = this;
            }

            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.Initialize(ContentFrame);
            navigationService.NavigateTo(typeof(Views.Pages.FeedView));
        }


        // pageType: can be any page (FeedPage, FullViewPage) and represents the next page the frame needs to load
        // parameter: is the data that needs to be displayed on this new page (and can be optional, which why we use "null")
        public void NavigateToPage(Type pageType, object parameter = null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }

        
    }
}