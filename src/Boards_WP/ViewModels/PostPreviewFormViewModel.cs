using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Boards_WP.ViewModels
{
    public partial class PostPreviewViewModel : ObservableObject
    {
        private readonly IPostsService postsService;
        private readonly UserSession userSession;
        private readonly MainViewModel mainViewModel;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedDate))]
        [NotifyPropertyChangedFor(nameof(DescriptionSnippet))]
        [NotifyPropertyChangedFor(nameof(PostImageSource))]
        [NotifyPropertyChangedFor(nameof(PostImageVisibility))]
        private Post postData;

        [ObservableProperty]
        private string communityName;

        [ObservableProperty]
        private string authorUsername;

        public BitmapImage PostImageSource => ConvertToBitmap(PostData?.Image);
        public Visibility PostImageVisibility => PostData?.Image?.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

        public string FormattedDate
        {
            get
            {
                if (PostData == null)
                {
                    return string.Empty;
                }

                var elapsed = DateTime.Now - PostData.CreationTime;

                if (elapsed.TotalMinutes < 1)
                {
                    return "just now";
                }

                if (elapsed.TotalHours < 1)
                {
                    return $"{(int)elapsed.TotalMinutes}m ago";
                }

                if (elapsed.TotalHours < 24)
                {
                    return $"{(int)elapsed.TotalHours}h ago";
                }

                return PostData.CreationTime.ToString("dd/MM/yyyy");
            }
        }

        public string DescriptionSnippet
        {
            get
            {
                if (string.IsNullOrEmpty(PostData?.Description))
                {
                    return string.Empty;
                }

                return PostData.Description.Length > 300
                    ? PostData.Description.Substring(0, 300) + "..."
                    : PostData.Description;
            }
        }

        public PostPreviewViewModel(
            Post post,
            IPostsService postsService,
            UserSession userSession,
            MainViewModel mainViewModel)
        {
            postData = post;
            this.postsService = postsService;
            this.userSession = userSession;
            this.mainViewModel = mainViewModel;
            communityName = post.ParentCommunity?.Name ?? "Unknown";
            authorUsername = post.Owner?.Username ?? "Unknown";
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
            if (PostData == null)
            {
                return;
            }

            var userId = userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0)
            {
                return;
            }

            postsService.IncreaseScore(PostData.PostID);
            postsService.UpdateUserInterests(userId, PostData, VoteType.Like, false);

            var updatedPost = postsService.GetPostByPostID(PostData.PostID);
            if (updatedPost != null)
            {
                PostData.Score = updatedPost.Score;

                OnPropertyChanged(nameof(PostData));
            }

            var newThemeColor = postsService.DetermineFeedThemeColorByLastLikes();
            mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void Downvote()
        {
            if (PostData == null)
            {
                return;
            }

            var userId = userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0)
            {
                return;
            }

            postsService.DecreaseScore(PostData.PostID);
            postsService.UpdateUserInterests(userId, PostData, VoteType.Dislike, false);

            var updatedPost = postsService.GetPostByPostID(PostData.PostID);
            if (updatedPost != null)
            {
                PostData.Score = updatedPost.Score;

                OnPropertyChanged(nameof(PostData));
            }

            var newThemeColor = postsService.DetermineFeedThemeColorByLastLikes();
            mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void OpenPost()
        {
            if (PostData == null)
            {
                return;
            }

            if (App.Current is App myApp && myApp.M_window is MainWindow mainWindow)
            {
                mainWindow.NavigateToPage(typeof(Views.Pages.FullPostView), PostData);
            }
        }
    }
}