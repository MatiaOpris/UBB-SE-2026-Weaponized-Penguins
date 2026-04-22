using Boards_WP.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views
{
    public sealed partial class NotificationsView : UserControl
    {
        public NotificationsListViewModel ViewModel { get; private set; }

        public NotificationsView()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                this.ViewModel = App.GetService<NotificationsListViewModel>();
                this.DataContext = this.ViewModel;
                this.Bindings.Update(); //--if you see an error here ignore it and just build, the app *should run ok
            };
        }

        public Visibility GetVisibility(bool isVisible)
            => isVisible ? Visibility.Visible : Visibility.Collapsed;

        private void NotificationsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NotificationItemViewModel itemViewModel)
            {
                itemViewModel.OpenNotificationCommand.Execute(null);
            }
        }
    }
}