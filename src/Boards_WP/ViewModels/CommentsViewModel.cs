using System;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Boards_WP.ViewModels
{
    public partial class CommentViewModel : ObservableObject
    {
        [ObservableProperty]
        private Comment _commentData;

        [ObservableProperty]
        private bool _isReplyAreaVisible = false;

        [ObservableProperty]
        private string _replyText = string.Empty;

        [ObservableProperty]
        private bool _isShareVisible;

        [ObservableProperty]
        private string _selectedChatName;

        public string CommentText => CommentData?.Description;

        private int _communityAdminId;

        public Visibility ActionButtonsVisibility =>
    (CommentData == null || CommentData.IsDeleted) ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<string> HardcodedChats { get; } = new()
        {
            "General Chat", "Sports Fans", "Tech Talk", "Weaponized Penguins Team"
        };


        public Visibility DeleteButtonVisibility
        {
            get
            {
                if (CommentData == null || _userSession?.CurrentUser == null) return Visibility.Collapsed;

                
                if (CommentData.IsDeleted) return Visibility.Collapsed;

                int currentUserId = _userSession.CurrentUser.UserID;

                bool isOwner = CommentData.Owner?.UserID == currentUserId;
                bool isAdmin = _communityAdminId == currentUserId;

                return (isOwner || isAdmin) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public IRelayCommand DeleteCommentCommand { get; }


        [RelayCommand]
        private void ToggleShare()
        {
            IsShareVisible = !IsShareVisible;
            if (IsShareVisible)
            {
                IsReplyAreaVisible = false;
            }
        }

        [RelayCommand]
        private void SendShare()
        {
            System.Diagnostics.Debug.WriteLine($"Sharing comment to: {SelectedChatName}");

            IsShareVisible = false;
            SelectedChatName = string.Empty;
        }

        public Action<Comment, string> ReplySubmitted { get; set; }

        public IRelayCommand UpvoteCommand { get; }
        public IRelayCommand DownvoteCommand { get; }
        public IRelayCommand ToggleReplyCommand { get; }
        public IRelayCommand SubmitReplyCommand { get; }

        private readonly ICommentsService _commentsService;
        private readonly UserSession _userSession;

        public SolidColorBrush UpvoteColor => CommentData?.UserCurrentVote == VoteType.Like 
            ? new SolidColorBrush(Colors.OrangeRed) 
            : new SolidColorBrush(Colors.Gray);

        public SolidColorBrush DownvoteColor => CommentData?.UserCurrentVote == VoteType.Dislike 
            ? new SolidColorBrush(Colors.CornflowerBlue) 
            : new SolidColorBrush(Colors.Gray);

        public CommentViewModel(Comment comment, int communityAdminId)
        {
            CommentData = comment;
            _communityAdminId = communityAdminId;
            _commentsService = App.Services?.GetService<ICommentsService>();
            _userSession = App.Services?.GetService<UserSession>();

            UpvoteCommand = new RelayCommand(() => {
                if (_commentsService != null && _userSession != null)
                {
                    _commentsService.IncreaseScore(CommentData, _userSession.CurrentUser.UserID);
                    OnPropertyChanged(nameof(CommentData));
                    OnPropertyChanged(nameof(UpvoteColor));
                    OnPropertyChanged(nameof(DownvoteColor));
                }
            });

            DownvoteCommand = new RelayCommand(() => {
                if (_commentsService != null && _userSession != null)
                {
                    _commentsService.DecreaseScore(CommentData, _userSession.CurrentUser.UserID);
                    OnPropertyChanged(nameof(CommentData));
                    OnPropertyChanged(nameof(UpvoteColor));
                    OnPropertyChanged(nameof(DownvoteColor));
                }
            });

            ToggleReplyCommand = new RelayCommand(() => IsReplyAreaVisible = !IsReplyAreaVisible);

            SubmitReplyCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrWhiteSpace(ReplyText))
                {
                    ReplySubmitted?.Invoke(CommentData, ReplyText);
                    ReplyText = string.Empty;
                    IsReplyAreaVisible = false;
                }
            });


            DeleteCommentCommand = new RelayCommand(() =>
            {
                if (_commentsService != null && _userSession != null && !CommentData.IsDeleted)
                {
                    _commentsService.SoftDeleteComment(CommentData, _userSession.CurrentUser.UserID);

                    
                    OnPropertyChanged(nameof(CommentText));
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                    OnPropertyChanged(nameof(ActionButtonsVisibility));
                }
            });
        }
    }
}