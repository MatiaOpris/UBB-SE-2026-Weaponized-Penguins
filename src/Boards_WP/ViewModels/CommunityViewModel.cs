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

        public MainViewModel MainViewModel => _mainViewModel;

        private readonly Action<Community> _navigateToCreatePost;
        private readonly Action<Community> _navigateToEditCommunity;

        private static ObservableCollection<Community> _sidebarList;

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
          _communitiesService.RemoveUser(CurrentCommunity.CommunityID, _userSession.CurrentUser.UserID);

            if (_communitiesService.CheckOwner(CurrentCommunity.CommunityID, _userSession.CurrentUser.UserID))
            {
                return;
            }

            _communitiesService.RemoveUser(CurrentCommunity.CommunityID, _userSession.CurrentUser.UserID);

            IsMember = false;
            CurrentCommunity.MembersNumber--;
            OnPropertyChanged(nameof(MemberCountText));

            //App.GetService<CommunityBarViewModel>().LoadCommunities();
            _sidebarList?.Remove(CurrentCommunity); 
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

                if (refreshedCommunity != null)
                {
                    CurrentCommunity = refreshedCommunity;
                }
                else
                {
                    CurrentCommunity = community;
                }

                var userId = _userSession.CurrentUser.UserID;
                IsOwner = _communitiesService.CheckOwner(community.CommunityID, userId);
                IsMember = IsOwner || _communitiesService.IsPartOfCommunity(userId, community.CommunityID);
                OnPropertyChanged(nameof(MemberCountText));
                OnPropertyChanged(nameof(BannerImage));
                OnPropertyChanged(nameof(ProfileImage));
                LoadPosts(community.CommunityID);
            }
            
        }

        private void LoadPosts(int communityId)
        {
            CommunityPosts.Clear();
            var posts = _postsService.GetPostsByCommunityID(communityId);
            if (posts == null) return;
            var mainViewModel = App.Services?.GetService<MainViewModel>();
            foreach (var post in posts)
            {
                var previewVm = new PostPreviewViewModel(post, _postsService, _userSession, mainViewModel);
                CommunityPosts.Add(previewVm);
            }
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