using System;
using System.Collections.ObjectModel;

using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Models;
using Boards_WP.Views.Pages; 

namespace Boards_WP.Views
{
    public sealed partial class NotificationsView : UserControl
    {
        public NotificationsListViewModel ViewModel { get; }

        public NotificationsView()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<ViewModels.NotificationsListViewModel>();
            NotificationsList.ItemsSource = ViewModel.Notifications;
        }

        private void NotificationsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NotificationItemViewModel itemViewModel)
            {
                itemViewModel.OpenNotificationCommand.Execute(null);
            }
        }
    }
}