using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Boards_WP.ViewModels
{
    public partial class CommunityViewModel : ObservableObject
    {
        private readonly IPostsService _postsService;
        private readonly ICommunitiesService _communitiesService;
        private readonly UserSession _userSession;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private ThemeColor _communityTheme;
        public MainViewModel MainViewModel => _mainViewModel;

        private readonly Action<Community> _navigateToCreatePost;
        private readonly Action<Community> _navigateToEditCommunity;

        private int _currentOffset = 0;
        private const int PageSize = 200; //--PAGINATION

        [ObservableProperty]
        private bool _hasMorePosts = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(BannerImage),
            nameof(ProfileImage),
            nameof(MemberCountText))]
        private Community _currentCommunity;

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(JoinButtonVisibility),
            nameof(MemberActionsVisibility))]
        [NotifyCanExecuteChangedFor(
            nameof(JoinCommand),
            nameof(LeaveCommand),
            nameof(CreatePostCommand))]
        private bool _isMember;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EditButtonVisibility))]
        [NotifyCanExecuteChangedFor(nameof(EditCommunityCommand))]
        private bool _isOwner;


        public ObservableCollection<PostPreviewViewModel> CommunityPosts { get; } = new();

        public BitmapImage BannerImage => ConvertToBitmap(CurrentCommunity?.Banner);
        public BitmapImage ProfileImage => ConvertToBitmap(CurrentCommunity?.Picture);
        public string MemberCountText => $"{CurrentCommunity?.MembersNumber ?? 0} members";

        public Visibility JoinButtonVisibility => IsMember ? Visibility.Collapsed : Visibility.Visible;
        public Visibility MemberActionsVisibility => IsMember ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditButtonVisibility => IsOwner ? Visibility.Visible : Visibility.Collapsed;

        public CommunityViewModel(Action<Community> navigateToCreatePost, Action<Community> navigateToEditCommunity)
        {
            _postsService = App.Services?.GetService<IPostsService>();
            _communitiesService = App.Services?.GetService<ICommunitiesService>();
            _userSession = App.Services?.GetService<UserSession>();
            _mainViewModel = App.Services?.GetService<MainViewModel>();
            _navigateToCreatePost = navigateToCreatePost;
            _navigateToEditCommunity = navigateToEditCommunity;
        }

        [RelayCommand(CanExecute = nameof(CanJoin))]
        private void Join()
        {
            _communitiesService.AddUser(CurrentCommunity.CommunityID, _userSession.CurrentUser.UserID);

            IsMember = true;
            CurrentCommunity.MembersNumber++;
            OnPropertyChanged(nameof(MemberCountText));

            App.GetService<CommunityBarViewModel>().LoadCommunities();
        }
        private bool CanJoin() => !IsMember;

        [RelayCommand(CanExecute = nameof(CanLeave))]
        private void Leave()
        {
            var userId = _userSession.CurrentUser.UserID;
            _communitiesService.RemoveUser(CurrentCommunity.CommunityID, userId);

            if (_communitiesService.CheckOwner(CurrentCommunity.CommunityID, userId))
            {
                return;
            }

            _communitiesService.RemoveUser(CurrentCommunity.CommunityID, userId);

            IsMember = false;
            CurrentCommunity.MembersNumber--;
            OnPropertyChanged(nameof(MemberCountText));

            App.GetService<CommunityBarViewModel>().LoadCommunities();
        }
        private bool CanLeave() => IsMember && !IsOwner;

        [RelayCommand(CanExecute = nameof(CanCreatePost))]
        private void CreatePost() => _navigateToCreatePost?.Invoke(CurrentCommunity);
        private bool CanCreatePost() => IsMember && CurrentCommunity != null;

        [RelayCommand(CanExecute = nameof(CanEditCommunity))]
        private void EditCommunity() => _navigateToEditCommunity?.Invoke(CurrentCommunity);
        private bool CanEditCommunity() => IsOwner && CurrentCommunity != null;

        public void ApplyNavigationParameter(object parameter)
        {
            if (parameter is Community community)
            {
                var refreshedCommunity = _communitiesService.GetCommunityByID(community.CommunityID);
                CurrentCommunity = refreshedCommunity ?? community;

                UpdateCommunityTheme();

                _currentOffset = 0;
                HasMorePosts = true;
                CommunityPosts.Clear();
                LoadBatch();

                var userId = _userSession.CurrentUser.UserID;
                IsOwner = _communitiesService.CheckOwner(CurrentCommunity.CommunityID, userId);

                IsMember = IsOwner || _communitiesService.IsPartOfCommunity(userId, CurrentCommunity.CommunityID);
            }  
        }

        private void UpdateCommunityTheme()
        {
            if (CurrentCommunity != null)
            {
                
                _communityTheme = _communitiesService.DetermineCommunityThemeColor(CurrentCommunity.CommunityID);
            }
        }

        [RelayCommand]
        public void LoadBatch()
        {
            if (CurrentCommunity == null || !HasMorePosts) return;

            int[] communityIds = new[] { CurrentCommunity.CommunityID };

            var posts = _postsService.GetPostsByCommunityIDs(communityIds, _currentOffset, PageSize);

            if (posts != null && posts.Count > 0)
            {
                var mainViewModel = App.GetService<MainViewModel>();
                foreach (var post in posts)
                {
                    var previewVm = new PostPreviewViewModel(post, _postsService, _userSession, mainViewModel);
                    CommunityPosts.Add(previewVm);
                }
                _currentOffset += posts.Count;
            }

            HasMorePosts = (posts?.Count == PageSize);
        }


        private static BitmapImage ConvertToBitmap(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(data);
            bitmap.SetSource(ms.AsRandomAccessStream());
            return bitmap;
        }
    }
}