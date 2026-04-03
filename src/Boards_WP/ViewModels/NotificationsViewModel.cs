using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Windows.UI.Text;

namespace Boards_WP.ViewModels;

public partial class NotificationItemViewModel : ObservableObject
{
    private readonly INotificationsService _notificationsService = App.GetService<INotificationsService>();
    private readonly INavigationService _navigationService = App.GetService<INavigationService>();

    public Notification NotificationData { get; }

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private string _time;

    [ObservableProperty]
    private bool _isUnread;

    public FontWeight MessageFontWeight => IsUnread ? FontWeights.Bold : FontWeights.Normal;

    public NotificationItemViewModel(Notification notification)
    {
        NotificationData = notification;


        if (_notificationsService != null)
        {
            Message = _notificationsService.GetNotificationMessage(notification);
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

        _time = notification.CreationTime.ToString("HH:mm");
        _isUnread = !notification.IsRead;
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
            if (_notificationsService != null)
            {
                _notificationsService.ReadNotification(NotificationData);
            }
            NotificationData.IsRead = true;
            IsUnread = false; 
        }

        if (NotificationData.RelatedPost != null && NotificationData.ActionType != NotificationType.PostDeleted)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateTo(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
            else if (Microsoft.UI.Xaml.Application.Current is App myApp && myApp.m_window is MainWindow mainWindow)
            {
                // just in case
                mainWindow.NavigateToPage(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
        }
    }
}

public partial class NotificationsListViewModel : ObservableObject
{
    private readonly INotificationsService _notificationsService = App.GetService<INotificationsService>();
    private readonly MainViewModel _mainViewModel;
    private readonly UserSession _userSession = App.GetService<UserSession>();

    private int _currentOffset = 0;
    private const int PageSize = 100; //--PAGINATION

    [ObservableProperty]
    private bool _hasMore = true;

    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

    public MainViewModel MainViewModel => _mainViewModel;

    public NotificationsListViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        LoadInitial(_userSession.CurrentUser.UserID);
    }

    public void LoadInitial(int userId)
    {
        _currentOffset = 0;
        Notifications.Clear();
        LoadBatch();
    }


    [RelayCommand]
    public void LoadBatch()
    {
        var userId = _userSession.CurrentUser.UserID;
        var data = _notificationsService.GetNotificationsByUserId(userId, _currentOffset, PageSize);

        if (data != null && data.Count > 0)
        {
            foreach (var n in data)
            {
                Notifications.Add(new NotificationItemViewModel(n));
            }
            _currentOffset += data.Count;
        }

        HasMore = (data?.Count == PageSize);
    }
}