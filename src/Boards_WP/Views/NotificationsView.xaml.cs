using System;
using System.Collections.ObjectModel;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Models;
using Boards_WP.Views.Pages; 

namespace Boards_WP.Views
{
    public sealed partial class NotificationsView : UserControl
    {
        public ObservableCollection<NotificationItem> Notifications { get; set; } = new();

        public NotificationsView()
        {
            this.InitializeComponent(); 
            NotificationsList.ItemsSource = Notifications;

            Notifications.Add(new NotificationItem { Message = "Someone liked your post", Time = "09:37" });
            Notifications.Add(new NotificationItem { Message = "Someone replied to your comment", Time = "08:58" });
            Notifications.Add(new NotificationItem { Message = "Someone liked your post", Time = "08:18" });
        }

        private void NotificationsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NotificationItem clickedNotification)
            {
                var mockPost = new Post
                {
                    PostID = 123,
                    Title = "Post from Notification",
                    Description = $"Content related to: {clickedNotification.Message}",
                    Owner = new User { Username = "@AuthorUser" },
                    CreationTime = DateTime.Now
                };

                Frame rootFrame = null;
                if (App.Current is App myApp && myApp.m_window != null)
                {
                    if (myApp.m_window.Content is FrameworkElement rootElement)
                    {
                        rootFrame = rootElement.FindName("ContentFrame") as Frame;
                    }
                }

                if (rootFrame != null)
                {
                    rootFrame.Navigate(typeof(FullPostView), mockPost);
                }
            }
        }
    }

    public class NotificationItem
    {
        public string Message { get; set; }
        public string Time { get; set; }
    }
}