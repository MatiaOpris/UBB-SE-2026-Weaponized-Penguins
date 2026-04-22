using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Boards_WP.ViewModels
{
    public partial class CommunityViewModel : ObservableObject
    {
        private readonly IPostsService postsService;
        private readonly ICommunitiesService communitiesService;
        private readonly UserSession userSession;
        private readonly MainViewModel mainViewModel;

        [ObservableProperty]
        private ThemeColor communityTheme;
        public MainViewModel MainViewModel => mainViewModel;

        private readonly Action<Community> navigateToCreatePost;
        private readonly Action<Community> navigateToEditCommunity;

        private int currentOffset = 0;
        private const int PageSize = 200; //--PAGINATION

        [ObservableProperty]
        private bool hasMorePosts = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(BannerImage),
            nameof(ProfileImage),
            nameof(MemberCountText))]
        private Community currentCommunity;

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(JoinButtonVisibility),
            nameof(MemberActionsVisibility))]
        [NotifyCanExecuteChangedFor(
            nameof(JoinCommand),
            nameof(LeaveCommand),
            nameof(CreatePostCommand))]
        private bool isMember;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EditButtonVisibility))]
        [NotifyCanExecuteChangedFor(nameof(EditCommunityCommand))]
        private bool isOwner;

        public ObservableCollection<PostPreviewViewModel> CommunityPosts { get; } = new ();

        public BitmapImage BannerImage => ConvertToBitmap(CurrentCommunity?.Banner);
        public BitmapImage ProfileImage => ConvertToBitmap(CurrentCommunity?.Picture);
        public string MemberCountText => $"{CurrentCommunity?.MembersNumber ?? 0} members";

        public Visibility JoinButtonVisibility => IsMember ? Visibility.Collapsed : Visibility.Visible;
        public Visibility MemberActionsVisibility => IsMember ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditButtonVisibility => IsOwner ? Visibility.Visible : Visibility.Collapsed;

        public CommunityViewModel(Action<Community> navigateToCreatePost, Action<Community> navigateToEditCommunity)
        {
            postsService = App.Services?.GetService<IPostsService>();
            communitiesService = App.Services?.GetService<ICommunitiesService>();
            userSession = App.Services?.GetService<UserSession>();
            mainViewModel = App.Services?.GetService<MainViewModel>();
            this.navigateToCreatePost = navigateToCreatePost;
            this.navigateToEditCommunity = navigateToEditCommunity;
        }

        [RelayCommand(CanExecute = nameof(CanJoin))]
        private void Join()
        {
            communitiesService.AddUser(CurrentCommunity.CommunityID, userSession.CurrentUser.UserID);

            IsMember = true;
            CurrentCommunity.MembersNumber++;
            OnPropertyChanged(nameof(MemberCountText));

            App.GetService<CommunityBarViewModel>().LoadCommunities();
        }
        private bool CanJoin() => !IsMember;

        [RelayCommand(CanExecute = nameof(CanLeave))]
        private void Leave()
        {
            var userId = userSession.CurrentUser.UserID;
            communitiesService.RemoveUser(CurrentCommunity.CommunityID, userId);

            if (communitiesService.CheckOwner(CurrentCommunity.CommunityID, userId))
            {
                return;
            }

            communitiesService.RemoveUser(CurrentCommunity.CommunityID, userId);

            IsMember = false;
            CurrentCommunity.MembersNumber--;
            OnPropertyChanged(nameof(MemberCountText));

            App.GetService<CommunityBarViewModel>().LoadCommunities();
        }
        private bool CanLeave() => IsMember && !IsOwner;

        [RelayCommand(CanExecute = nameof(CanCreatePost))]
        private void CreatePost() => navigateToCreatePost?.Invoke(CurrentCommunity);
        private bool CanCreatePost() => IsMember && CurrentCommunity != null;

        [RelayCommand(CanExecute = nameof(CanEditCommunity))]
        private void EditCommunity() => navigateToEditCommunity?.Invoke(CurrentCommunity);
        private bool CanEditCommunity() => IsOwner && CurrentCommunity != null;

        public void ApplyNavigationParameter(object parameter)
        {
            if (parameter is Community community)
            {
                var refreshedCommunity = communitiesService.GetCommunityByID(community.CommunityID);
                CurrentCommunity = refreshedCommunity ?? community;

                UpdateCommunityTheme();

                currentOffset = 0;
                HasMorePosts = true;
                CommunityPosts.Clear();
                LoadBatch();

                var userId = userSession.CurrentUser.UserID;
                IsOwner = communitiesService.CheckOwner(CurrentCommunity.CommunityID, userId);

                IsMember = IsOwner || communitiesService.IsPartOfCommunity(userId, CurrentCommunity.CommunityID);
            }
        }

        private void UpdateCommunityTheme()
        {
            if (CurrentCommunity != null)
            {
                communityTheme = communitiesService.DetermineCommunityThemeColor(CurrentCommunity.CommunityID);
            }
        }

        [RelayCommand]
        public void LoadBatch()
        {
            if (CurrentCommunity == null || !HasMorePosts)
            {
                return;
            }

            int[] communityIds = new[] { CurrentCommunity.CommunityID };

            var posts = postsService.GetPostsByCommunityIDs(communityIds, currentOffset, PageSize);

            if (posts != null && posts.Count > 0)
            {
                var mainViewModel = App.GetService<MainViewModel>();
                foreach (var post in posts)
                {
                    var previewVm = new PostPreviewViewModel(post, postsService, userSession, mainViewModel);
                    CommunityPosts.Add(previewVm);
                }
                currentOffset += posts.Count;
            }

            HasMorePosts = (posts?.Count == PageSize);
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
    }
}