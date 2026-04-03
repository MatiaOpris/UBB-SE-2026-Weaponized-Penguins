using System.Collections.ObjectModel;
using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;

namespace Boards_WP.ViewModels
{
    public partial class FullPostViewModel : ObservableObject
    {
        
        private readonly IPostsService _postsService;
        private readonly ICommentsService _commentsService;
        private readonly MainViewModel _mainViewModel;
        private readonly UserSession _userSession;

        public MainViewModel MainViewModel => _mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PostImageSource))]
        [NotifyPropertyChangedFor(nameof(PostImageVisibility))]
        [NotifyPropertyChangedFor(nameof(AuthorUsername))]
        [NotifyPropertyChangedFor(nameof(CurrentPostTags))]
        private Post _currentPost;

        [ObservableProperty]
        private string _newCommentText;

        [ObservableProperty]
        private bool _isCommentAreaVisible;

        [ObservableProperty]
        private bool _isShareAreaVisible;

        [ObservableProperty]
        private string _selectedChatName;

        public ObservableCollection<string> HardcodedChats { get; } = new()
        {
            "General Chat", "Sports Fans", "Tech Talk", "Weaponized Penguins Team"
        };

        [RelayCommand]
        private void ToggleShareArea()
        {
            IsShareAreaVisible = !IsShareAreaVisible;
            if (IsShareAreaVisible)
            {
                IsCommentAreaVisible = false; 
            }
        }

        [RelayCommand]
        private void SendShare()
        {
            IsShareAreaVisible = false;
            SelectedChatName = string.Empty;
        }

        public BitmapImage PostImageSource => ConvertToBitmap(CurrentPost?.Image);
        public Visibility PostImageVisibility => CurrentPost?.Image?.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        public string AuthorUsername => CurrentPost?.Owner?.Username ?? "Unknown";
        public IEnumerable<Tag> CurrentPostTags => CurrentPost?.Tags ?? new List<Tag>();

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
                PostComments.Add(c);
        }

        private static BitmapImage ConvertToBitmap(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(data);
            bitmap.SetSource(ms.AsRandomAccessStream());
            return bitmap;
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

            
            var newThemeColor = _postsService.DetermineThemeForASinglePost(updatedPost);
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

                _postsService.IncreaseCommentsNumber(CurrentPost.PostID);
                OnPropertyChanged(nameof(CurrentPost));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}