using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Boards_WP.ViewModels
{
    public partial class FullPostViewModel : ObservableObject
    {
        private readonly IPostsService postsService;
        private readonly ICommentsService commentsService;
        private readonly MainViewModel mainViewModel;
        private readonly UserSession userSession;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PostImageSource))]
        [NotifyPropertyChangedFor(nameof(PostImageVisibility))]
        [NotifyPropertyChangedFor(nameof(AuthorUsername))]
        [NotifyPropertyChangedFor(nameof(CurrentPostTags))]
        private Post currentPost;

        [ObservableProperty]
        private string newCommentText;

        [ObservableProperty]
        private bool isCommentAreaVisible;

        [ObservableProperty]
        private bool isShareAreaVisible;

        [ObservableProperty]
        private string selectedChatName;

        [ObservableProperty]
        private bool canDeletePost;

        private VoteType finalVote = VoteType.None;
        private bool hasCommented = false;

        public ObservableCollection<string> HardcodedChats { get; } = new ()
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

        public ObservableCollection<Comment> PostComments { get; } = new ();

        public FullPostViewModel(
            IPostsService postsService,
            ICommentsService commentsService,
            MainViewModel mainViewModel,
            UserSession userSession)
        {
            this.postsService = postsService;
            this.commentsService = commentsService;
            this.mainViewModel = mainViewModel;
            this.userSession = userSession;
        }

        public void Initialize(Post post)
        {
            var fullPost = postsService.GetPostByPostID(post.PostID);
            CurrentPost = fullPost ?? post;

            if (CurrentPost != null && userSession.CurrentUser != null)
            {
                int currentUserId = userSession.CurrentUser.UserID;

                bool isOwner = CurrentPost.Owner?.UserID == currentUserId;

                bool isAdmin = CurrentPost.ParentCommunity?.Admin?.UserID == currentUserId;

                canDeletePost = isOwner || isAdmin;
            }

            LoadComments();
        }

        [RelayCommand]
        private void DeletePost()
        {
            if (CurrentPost == null)
            {
                return;
            }

            try
            {
                postsService.DeletePost(CurrentPost.PostID);

                var navService = App.Services.GetService<INavigationService>();
                if (navService != null)
                {
                    navService.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete post: {ex.Message}");
            }
        }

        private void LoadComments()
        {
            PostComments.Clear();
            if (CurrentPost == null)
            {
                return;
            }

            var userId = userSession.CurrentUser?.UserID ?? 0;

            var comments = commentsService.GetCommentsByPost(CurrentPost.PostID, userId);

            foreach (var c in comments)
            {
                PostComments.Add(c);
            }
        }

        private static BitmapImage ConvertToBitmap(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(data);
            bitmap.SetSource(ms.AsRandomAccessStream());
            return bitmap;
        }

        [RelayCommand]
        private void Upvote()
        {
            if (CurrentPost == null)
            {
                return;
            }

            var userId = userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0)
            {
                return;
            }

            postsService.IncreaseScore(CurrentPost.PostID);
            // _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Like, false);
            var updatedPost = postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost));
            }

            var newThemeColor = postsService.DetermineThemeForASinglePost(updatedPost);
            mainViewModel.ApplyNewTheme(newThemeColor);
            finalVote = VoteType.Like;
        }

        [RelayCommand]
        private void Downvote()
        {
            if (CurrentPost == null)
            {
                return;
            }

            var userId = userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0)
            {
                return;
            }

            postsService.DecreaseScore(CurrentPost.PostID);
            // _postsService.UpdateUserInterests(userId, CurrentPost, VoteType.Dislike, false);
            var updatedPost = postsService.GetPostByPostID(CurrentPost.PostID);
            if (updatedPost != null)
            {
                CurrentPost.Score = updatedPost.Score;
                OnPropertyChanged(nameof(CurrentPost));
            }

            var newThemeColor = postsService.DetermineFeedThemeColorByLastLikes();
            mainViewModel.ApplyNewTheme(newThemeColor);
            finalVote = VoteType.Dislike;
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
            if (string.IsNullOrWhiteSpace(NewCommentText) || CurrentPost == null)
            {
                return;
            }

            var newComment = new Comment
            {
                ParentPost = CurrentPost,
                Owner = userSession.CurrentUser,
                Description = NewCommentText,
                CreationTime = DateTime.Now
            };

            try
            {
                commentsService.AddComment(newComment);
                PostComments.Insert(0, newComment);
                CurrentPost.CommentsNumber++;
                NewCommentText = string.Empty;
                IsCommentAreaVisible = false;

                postsService.IncreaseCommentsNumber(CurrentPost.PostID);
                OnPropertyChanged(nameof(CurrentPost));
                hasCommented = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void OnExitView()
        {
            if (CurrentPost == null || userSession.CurrentUser == null)
            {
                return;
            }

            postsService.UpdateUserInterests(
                userSession.CurrentUser.UserID,
                CurrentPost,
                finalVote,
                hasCommented);
        }
    }
}
