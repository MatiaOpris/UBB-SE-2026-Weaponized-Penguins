using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class CreateCommunityViewModel : ObservableObject
    {
        private readonly ICommunitiesService communitiesService;
        private readonly INavigationService navigationService;
        private readonly UserSession userSession;
        private readonly MainViewModel mainViewModel;

        public MainViewModel MainViewModel => mainViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateCommunityCommand))]
        private string communityName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateCommunityCommand))]
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

        public CreateCommunityViewModel(
            ICommunitiesService communitiesService,
            INavigationService navigationService,
            UserSession userSession,
            MainViewModel mainViewModel)
        {
            this.communitiesService = communitiesService;
            this.navigationService = navigationService;
            this.userSession = userSession;
            this.mainViewModel = mainViewModel;
        }

        [RelayCommand(CanExecute = nameof(CanCreateCommunity))]
        private void CreateCommunity()
        {
            ErrorMessage = null;
            try
            {
                Community createdCommunity = new ()
                {
                    Name = CommunityName,
                    Description = CommunityDescription,
                    Picture = CommunityPicture,
                    Banner = CommunityBanner,
                    Admin = userSession.CurrentUser
                };

                communitiesService.AddCommunity(createdCommunity);
                SidebarList.Add(createdCommunity);
                navigationService.NavigateTo(typeof(CommunityView), createdCommunity);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Cancel() => navigationService.GoBack();

        private bool CanCreateCommunity() =>
            !string.IsNullOrWhiteSpace(CommunityName) &&
            !string.IsNullOrWhiteSpace(CommunityDescription);
    }
}
