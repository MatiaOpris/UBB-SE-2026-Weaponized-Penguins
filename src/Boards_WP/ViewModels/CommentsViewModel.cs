using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Boards_WP.Data.Models;

using System;

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

        public Action<Comment, string> ReplySubmitted { get; set; }

        public IRelayCommand UpvoteCommand { get; }
        public IRelayCommand DownvoteCommand { get; }
        public IRelayCommand ToggleReplyCommand { get; }
        public IRelayCommand SubmitReplyCommand { get; }

        public CommentViewModel(Comment comment)
        {
            CommentData = comment;

            UpvoteCommand = new RelayCommand(() => {
                CommentData.Score++;
                OnPropertyChanged(nameof(CommentData));
            });

            DownvoteCommand = new RelayCommand(() => {
                CommentData.Score--;
                OnPropertyChanged(nameof(CommentData));
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
        }
    }
}