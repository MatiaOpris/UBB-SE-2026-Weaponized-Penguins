using Microsoft.UI.Xaml;

using System;

namespace Boards_WP
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
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