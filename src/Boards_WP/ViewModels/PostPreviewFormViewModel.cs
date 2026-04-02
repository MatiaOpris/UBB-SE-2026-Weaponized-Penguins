using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces; 

namespace Boards_WP.ViewModels
{
    public partial class PostPreviewViewModel : ObservableObject
    {
        private readonly IPostsService _postsService;
        private readonly UserSession _userSession;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedDate))]
        [NotifyPropertyChangedFor(nameof(DescriptionSnippet))] 
        private Post _postData;

        [ObservableProperty]
        private string _communityName;

        [ObservableProperty]
        private string _authorUsername;

        public string FormattedDate => PostData?.CreationTime.ToString("dd/MM/yyyy") ?? "";

        public string DescriptionSnippet
        {
            get
            {
                if (string.IsNullOrEmpty(PostData?.Description)) return string.Empty;
                return PostData.Description.Length > 150 
                    ? PostData.Description.Substring(0, 150) + "..." 
                    : PostData.Description;
            }
        }


        public PostPreviewViewModel(
            Post post, 
            IPostsService postsService, 
            UserSession userSession,
            MainViewModel mainViewModel)
        {
            _postData = post;
            _postsService = postsService;
            _userSession = userSession;
            _mainViewModel = mainViewModel;
            _communityName = post.ParentCommunity?.Name ?? "Unknown";
            _authorUsername = post.Owner?.Username ?? "Unknown";
        }

        [RelayCommand]
        private void Upvote()
        {
            if (PostData == null) return;
            var userId = _userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;

            // 1. Tell the service to execute its logic
            _postsService.IncreaseScore(PostData.PostID);
            _postsService.UpdateUserInterests(userId, PostData, VoteType.Like, false);

            // 2. Fetch the true, calculated score back from the database
            var updatedPost = _postsService.GetPostByPostID(PostData.PostID);
            if (updatedPost != null)
            {
                PostData.Score = updatedPost.Score;
                OnPropertyChanged(nameof(PostData)); // Instantly update UI
            }

            // 3. Update the theme
            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);

        }

        [RelayCommand]
        private void Downvote()
        {
            if (PostData == null) return;
            var userId = _userSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;

            // 1. Tell the service to execute its logic
            _postsService.DecreaseScore(PostData.PostID);
            _postsService.UpdateUserInterests(userId, PostData, VoteType.Dislike, false);

            // 2. Fetch the true, calculated score back from the database
            var updatedPost = _postsService.GetPostByPostID(PostData.PostID);
            if (updatedPost != null)
            {
                PostData.Score = updatedPost.Score;
                OnPropertyChanged(nameof(PostData)); // Instantly update UI
            }

            // 3. Update the theme
            var newThemeColor = _postsService.DetermineFeedThemeColorByLastLikes();
            _mainViewModel.ApplyNewTheme(newThemeColor);
        }

        [RelayCommand]
        private void OpenPost()
        {
            if (PostData == null) return;

            if (App.Current is App myApp && myApp.m_window is MainWindow mainWindow)
                mainWindow.NavigateToPage(typeof(Views.Pages.FullPostView), PostData);
        }
    }
}