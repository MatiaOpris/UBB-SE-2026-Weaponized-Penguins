using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Boards_WP.ViewModels
{
    public partial class UpdateCommunityViewModel : ObservableObject
    {
        private readonly ICommunitiesService communitiesService;
        private readonly INavigationService navigationService;
        private readonly MainViewModel mainViewModel;
        private readonly UserSession userSession;

        private Community community = null!;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommunityCommand))]
        private string communityName = string.Empty;

        [ObservableProperty]
        private string communityDescription = string.Empty;

        private byte[] communityPicture = null!;

        [global::System.Diagnostics.CodeAnalysis.MaybeNull]
        public byte[] CommunityPicture
        {
            get => communityPicture;
            set => SetProperty(ref communityPicture, value!);
        }

        private byte[] communityBanner = null!;

        [global::System.Diagnostics.CodeAnalysis.MaybeNull]
        public byte[] CommunityBanner
        {
            get => communityBanner;
            set => SetProperty(ref communityBanner, value!);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string? errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ObservableCollection<Community> SidebarList { get; set; } = new ObservableCollection<Community>();

        public UpdateCommunityViewModel()
        {
            mainViewModel = App.Services?.GetService<MainViewModel>();
            communitiesService = App.Services?.GetService<ICommunitiesService>();
            navigationService = App.Services?.GetService<INavigationService>();
            userSession = App.Services?.GetService<UserSession>();
        }

        public void Initialize(Community community)
        {
            this.community = community;

            CommunityName = community.Name;
            CommunityDescription = community.Description;
            CommunityPicture = community.Picture;
            CommunityBanner = community.Banner;
        }

        [RelayCommand(CanExecute = nameof(CanUpdateCommunity))]
        private void UpdateCommunity()
        {
            ErrorMessage = null;

            try
            {
                community.Name = CommunityName;
                community.Description = CommunityDescription;
                community.Picture = CommunityPicture;
                community.Banner = CommunityBanner;

                communitiesService.UpdateCommunityInfo(community.CommunityID, CommunityDescription, CommunityPicture, CommunityBanner);

                navigationService.NavigateTo(typeof(CommunityView), community);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Cancel() => navigationService.GoBack();

        private bool CanUpdateCommunity() => !string.IsNullOrWhiteSpace(CommunityName);
    }
}
