using System.Collections.ObjectModel;
using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Boards_WP.ViewModels
{
    public partial class FullPostViewModel : ObservableObject
    {
        
        private readonly IPostsService _postsService;
        private readonly ICommentsService _commentsService;
        private readonly MainViewModel _mainViewModel;
        private readonly UserSession _userSession;

        [ObservableProperty]
        private Post _currentPost;

        [ObservableProperty]
        private string _newCommentText;

        [ObservableProperty]
        private bool _isCommentAreaVisible;

        public ObservableCollection<Comment> PostComments { get; } = new();

        public FullPostViewModel(
            IPostsService postsService,
            ICommentsService commentsService,
            MainViewModel mainViewModel,
            UserSession userSession)
        {
            _postsService = postsService;
            _commentsService = commentsService;
            _mainViewModel = mainViewModel;
            _userSession = userSession;
        }

        public void Initialize(Post post)
        {
            var fullPost = _postsService.GetPostByPostID(post.PostID);
            CurrentPost = fullPost ?? post;
            LoadComments();
        }

        private void LoadComments()
        {
            PostComments.Clear();
            if (CurrentPost == null) return;

            var userId = _userSession.CurrentUser?.UserID ?? 0;
            var comments = _commentsService.GetCommentsByPost(CurrentPost.PostID, userId);

            foreach (var c in comments)
            {
                PostComments.Add(c);
            }
        }

        [RelayCommand]
        private void Upvote()
        {
            if (CurrentPost == null) return;
            var userId = _userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;

            _postsService.IncreaseScore(CurrentPost.PostID);
            _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Like, false);

            var updatedPost = _postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost));
            }

            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void Downvote()
        {
            if (CurrentPost == null) return;
            var userId = _userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;

            _postsService.DecreaseScore(CurrentPost.PostID);
            _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Dislike, false);

            var updatedPost = _postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost));
            }

            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void ShowCommentArea()
        {
            IsCommentAreaVisible = true;
        }

        [RelayCommand]
        private void CancelComment()
        {
            IsCommentAreaVisible = false;
            NewCommentText = string.Empty;
        }

        
        [RelayCommand]
        private void PostComment()
        {
            if (string.IsNullOrWhiteSpace(NewCommentText) || CurrentPost == null) return;

            var newComment = new Comment
            {
                ParentPost = CurrentPost,
                Owner = _userSession.CurrentUser,
                Description = NewCommentText,
                CreationTime = DateTime.Now
            };

            try
            {
                _commentsService.AddComment(newComment);
                PostComments.Insert(0, newComment);
                CurrentPost.CommentsNumber++;
                NewCommentText = string.Empty;
                IsCommentAreaVisible = false;

                OnPropertyChanged(nameof(CurrentPost));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}