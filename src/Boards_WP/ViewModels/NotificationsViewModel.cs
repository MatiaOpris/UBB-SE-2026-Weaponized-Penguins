using System;
using System.Collections.ObjectModel;

using Boards_WP.Data;
using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories;
using Boards_WP.Data.Services;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Text;

using Windows.Networking.NetworkOperators;
using Windows.UI.Text;

namespace Boards_WP.ViewModels;

public partial class NotificationItemViewModel : ObservableObject
{
    private readonly INotificationsService _notificationsService;
    private readonly INavigationService _navigationService;

    public Notification NotificationData { get; }

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private string _time;

    [ObservableProperty]
    private bool _isUnread;

    public FontWeight MessageFontWeight => IsUnread ? FontWeights.Bold : FontWeights.Normal;

    public NotificationItemViewModel(Notification notification, INotificationsService notificationsService, INavigationService navigationService = null)
    {
        NotificationData = notification;
        _notificationsService = notificationsService;
        _navigationService = navigationService;

        if (_notificationsService != null)
        {
            _message = _notificationsService.GetNotificationMessage(notification);
        }
        else
        {
            // Mock get message
            string actorName = notification.Actor?.Username ?? "Someone";
            string postTitle = notification.RelatedPost?.Title ?? "a post";
            _message = notification.ActionType switch
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
            IsUnread = false; // This triggers MessageFontWeight update
        }

        if (NotificationData.RelatedPost != null && NotificationData.ActionType != NotificationType.PostDeleted)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateTo(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
            else if (Microsoft.UI.Xaml.Application.Current is App myApp && myApp.m_window is MainWindow mainWindow)
            {
                // Fallback just in case
                mainWindow.NavigateToPage(typeof(Views.Pages.FullPostView), NotificationData.RelatedPost);
            }
        }
    }
}

public partial class NotificationsListViewModel : ObservableObject
{
    private readonly INotificationsService _notificationsService;

    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

    private readonly MainViewModel _mainViewModel;
    private readonly UserSession _userSession;


    public MainViewModel MainViewModel => _mainViewModel;

    public NotificationsListViewModel(UserSession userSession, MainViewModel mainViewModel, INotificationsService notificationsService = null)
    {
        _notificationsService = notificationsService;
        _userSession = userSession;
        _mainViewModel = mainViewModel;
        LoadNotifications(1); // Assuming current user ID is 1 for now
    }

    public void LoadNotifications(int userId)
    {
        Notifications.Clear();
        if (_notificationsService != null)
        {
            var notifs = _notificationsService.GetNotificationsByUserID(userId);
            foreach (var n in notifs)
            {
                Notifications.Add(new NotificationItemViewModel(n, _notificationsService));
            }
        }
        else
        {
            // Adding mock data here if no service is provided
            var mockUser = new User { UserID = 1, Username = "TestUser" };
            var mockPost = new Post { PostID = 1, Title = "My Mock Post" };

            Notifications.Add(new NotificationItemViewModel(new Notification { NotificationID = 1, ActionType = NotificationType.CommentOnPost, Actor = new User { Username = "Alice" }, Receiver = mockUser, RelatedPost = mockPost, IsRead = false, CreationTime = DateTime.Now.AddMinutes(-5) }, null));
            Notifications.Add(new NotificationItemViewModel(new Notification { NotificationID = 2, ActionType = NotificationType.ReplyToComment, Actor = new User { Username = "Bob" }, Receiver = mockUser, RelatedPost = mockPost, IsRead = true, CreationTime = DateTime.Now.AddHours(-1) }, null));
            Notifications.Add(new NotificationItemViewModel(new Notification { NotificationID = 3, ActionType = NotificationType.PostDeleted, Actor = new User { Username = "Moderator" }, Receiver = mockUser, RelatedPost = mockPost, IsRead = false, CreationTime = DateTime.Now.AddDays(-1) }, null));
        }
    }
}