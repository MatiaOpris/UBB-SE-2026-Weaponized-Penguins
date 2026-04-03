using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels
{
    public partial class CreateCommunityViewModel : ObservableObject
    {
        private readonly ICommunitiesService _communitiesService;
        private readonly INavigationService _navigationService;
        private readonly UserSession _userSession;
        private readonly MainViewModel _mainViewModel;

        public MainViewModel MainViewModel => _mainViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateCommunityCommand))]
        private string _communityName = string.Empty;

        [ObservableProperty]
        private string _communityDescription = string.Empty;

        [ObservableProperty]
        private byte[]? _communityPicture;

        [ObservableProperty]
        private byte[]? _communityBanner;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ObservableCollection<Community> SidebarList { get; set; } = [];

        public CreateCommunityViewModel(
            ICommunitiesService communitiesService,
            INavigationService navigationService,
            UserSession userSession)
        {
            _communitiesService = communitiesService;
            _navigationService = navigationService;
            _userSession = userSession;
        }

        [RelayCommand(CanExecute = nameof(CanCreateCommunity))]
        private void CreateCommunity()
        {
            ErrorMessage = null;
            try
            {
                Community createdCommunity = new()
                {
                    Name = CommunityName,
                    Description = CommunityDescription,
                    Picture = CommunityPicture, 
                    Banner = CommunityBanner,     
                    Admin = _userSession.CurrentUser
                };

                _communitiesService.AddCommunity(createdCommunity);
                SidebarList.Add(createdCommunity);
                _navigationService.NavigateTo(typeof(CommunityView), createdCommunity);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Cancel() => _navigationService.GoBack();

        private bool CanCreateCommunity() => !string.IsNullOrWhiteSpace(CommunityName);
    }
}