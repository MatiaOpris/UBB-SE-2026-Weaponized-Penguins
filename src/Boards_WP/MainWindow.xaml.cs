using Microsoft.UI.Xaml;

using System;

namespace Boards_WP
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // tell the App class about this window so we can find it later (in case we need the main window)
            if (App.Current is App myApp)
            {
                myApp.m_window = this;
            }

            // loading the Feed by default when the app opens
            ContentFrame.Navigate(typeof(Views.Pages.FeedView));
        }
        
        // pageType: can be any page (FeedPage, FullViewPage) and represents the next page the frame needs to load
        // parameter: is the data that needs to be displayed on this new page (and can be optional, which why we use "null")
        public void NavigateToPage(Type pageType, object parameter = null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }
    }
}