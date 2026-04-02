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

            ContentFrame.Navigate(typeof(Views.Pages.FeedView));
        }

        //--TODO: I doubt that this still works (we have another navigation service now)
        public void NavigateToPage(Type pageType, object parameter = null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }

        
    }
}