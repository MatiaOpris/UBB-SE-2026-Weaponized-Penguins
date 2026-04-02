using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services; // Ensure this matches your namespace

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class FullPostViewModel : ObservableObject
    {
        // Dependencies
        private readonly IPostsService _postsService;
        private readonly ICommentsService _commentsService;
        private readonly MainViewModel _mainViewModel;
        private readonly UserSession _userSession;

        [ObservableProperty]
        private Post _currentPost;

        [ObservableProperty]
        private string _newCommentText;

        public ObservableCollection<Comment> PostComments { get; } = new();

        // 1. Constructor Injection (Strict MVVM)
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

        // 2. Initialization Method (Called by the View when navigated to)
        public void Initialize(Post post)
        {
            CurrentPost = post;
            LoadComments();
        }

        private void LoadComments()
        {
            PostComments.Clear();
            if (CurrentPost == null) return;

            var userId = _userSession.CurrentUser?.UserID ?? 0;

            // Fetch real data from the database
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

            // 1. Tell the service to execute its logic
            _postsService.IncreaseScore(CurrentPost.PostID);
            _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Like, false);

            // 2. Fetch the true, calculated score back from the database
            var updatedPost = _postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost)); // Instantly update UI
            }

            // 3. Update the theme
            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void Downvote()
        {
            if (CurrentPost == null) return;
            var userId = _userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;

            // 1. Tell the service to execute its logic
            _postsService.DecreaseScore(CurrentPost.PostID);
            _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Dislike, false);

            // 2. Fetch the true, calculated score back from the database
            var updatedPost = _postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost)); // Instantly update UI
            }

            // 3. Update the theme
            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void PostComment()
        {
            if (string.IsNullOrWhiteSpace(NewCommentText) || CurrentPost == null) return;

            var currentUser = _userSession.CurrentUser;
            if (currentUser == null) return;

            // 1. Create the Comment object here in the ViewModel
            var newComment = new Comment
            {
                ParentPost = CurrentPost,    // Required for Notifications
                Owner = currentUser,         // Required for Notifications
                Description = NewCommentText
                // Note: Do NOT set the Indentation or CreationTime here. 
                // Your Service is already handling that perfectly!
            };

            // 2. Pass the object to the service 
            _commentsService.AddComment(newComment);

            // 3. Update the UI
            PostComments.Insert(0, newComment);

            CurrentPost.CommentsNumber++;
            OnPropertyChanged(nameof(CurrentPost));

            // 4. Clear the textbox
            NewCommentText = string.Empty;
        }
    }
}