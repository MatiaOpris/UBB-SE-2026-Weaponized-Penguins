using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views
{
    public sealed partial class NotificationsView : UserControl
    {
        // Simple list for backend dev to plug in
        public ObservableCollection<NotificationItem> Notifications { get; set; } = new();

        public NotificationsView()
        {
            this.InitializeComponent();
            NotificationsList.ItemsSource = Notifications;

            // Mock Data
            Notifications.Add(new NotificationItem { Message = "Someone liked your post", Time = "09:37" });
            Notifications.Add(new NotificationItem { Message = "Someone replied to your comment", Time = "08:58" });
            Notifications.Add(new NotificationItem { Message = "Someone liked your post", Time = "08:18" });
        }
    }

    // This tiny class makes integration much easier for the backend dev
    public class NotificationItem
    {
        public string Message { get; set; }
        public string Time { get; set; }
    }
}