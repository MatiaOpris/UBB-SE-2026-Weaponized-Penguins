using Microsoft.UI.Xaml;

using System;
using System.Collections.Generic;

using Boards_WP.Data.Models;

namespace Boards_WP
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(Boards_WP.Views.Pages.FeedView));
        }
    }
}