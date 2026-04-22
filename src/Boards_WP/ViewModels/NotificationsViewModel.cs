using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Text;

using Windows.UI.Text;

namespace Boards_WP.ViewModels;

public partial class NotificationItemViewModel : ObservableObject
{
    private readonly INotificationsService notificationsService = App.GetService<INotificationsService>();
    private readonly INavigationService navigationService = App.GetService<INavigationService>();

    public Notification NotificationData { get; }

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private string time;

    [ObservableProperty]
    private bool isUnread;

    public FontWeight MessageFontWeight => IsUnread ? FontWeights.Bold : FontWeights.Normal;

    public NotificationItemViewModel(Notification notification)
    {
        NotificationData = notification;

        if (notificationsService != null)
        {
            Message = notificationsService.GetNotificationMessage(notification);
        }
        else
        {
            string actorName = notification.Actor?.Username ?? "Someone";
            string postTitle = notification.RelatedPost?.Title ?? "a post";
            Message = notification.ActionType switch
            {
                NotificationType.CommentOnPost => $"{actorName} commented on your post '{postTitle}'",
                NotificationType.ReplyToComment => $"{actorName} replied to your comment",
                NotificationType.PostDeleted => $"Your post '{postTitle}' was deleted",
                NotificationType.CommentDeleted => $"Your comment was deleted",
                _ => "New notification"
            };
        }

        time = notification.CreationTime.ToString("HH:mm");
        isUnread = !notification.IsRead;
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsUnread))
        {
            OnPropertyChanged(nameof(MessageFontWeight));
        }
    }

    [RelayCommand]
    private void OpenNotification()
    {
        if (IsUnread)
        {
            if (notificationsService != null)
            {
                notificationsService.ReadNotification(NotificationData);
            }
            NotificationData.IsRead = true;
            IsUnread = false;
        }

        if (NotificationData.RelatedPost != null && NotificationData.ActionType != NotificationType.PostDeleted)
        {
            if (navigationService != null)
            {
                navigationService.NavigateTo(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
            else if (Microsoft.UI.Xaml.Application.Current is App myApp && myApp.M_window is MainWindow mainWindow)
            {
                mainWindow.NavigateToPage(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
        }
    }
}

public partial class NotificationsListViewModel : ObservableObject
{
    private readonly INotificationsService notificationsService = App.GetService<INotificationsService>();
    private readonly MainViewModel mainViewModel;
    private readonly UserSession userSession = App.GetService<UserSession>();

    private int currentOffset = 0;
    private const int PageSize = 100; //--PAGINATION

    [ObservableProperty]
    private bool hasMore = true;

    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new ();

    public MainViewModel MainViewModel => mainViewModel;

    public NotificationsListViewModel(MainViewModel mainViewModel)
    {
        this.mainViewModel = mainViewModel;
        LoadInitial(userSession.CurrentUser.UserID);
    }

    public void LoadInitial(int userId)
    {
        currentOffset = 0;
        Notifications.Clear();
        LoadBatch();
    }

    [RelayCommand]
    public void LoadBatch()
    {
        var userId = userSession.CurrentUser.UserID;
        var data = notificationsService.GetNotificationsByUserId(userId, currentOffset, PageSize);

        if (data != null && data.Count > 0)
        {
            foreach (var n in data)
            {
                Notifications.Add(new NotificationItemViewModel(n));
            }
            currentOffset += data.Count;
        }

        HasMore = (data?.Count == PageSize);
    }
}