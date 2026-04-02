using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Boards_WP.ViewModels
{
    public partial class CreateCommunityViewModel : ObservableObject
    {
        private readonly ICommunitiesService _communitiesService;
        private readonly INavigationService _navigationService;
        private readonly MainViewModel _mainViewModel;
        private readonly UserSession _userSession;

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

        public CreateCommunityViewModel()
        {
            _mainViewModel = App.Services?.GetService<MainViewModel>();
            _communitiesService = App.Services?.GetService<ICommunitiesService>();
            _navigationService = App.Services?.GetService<INavigationService>();
            _userSession = App.Services?.GetService<UserSession>();
        }

        [RelayCommand(CanExecute = nameof(CanCreateCommunity))]
        private void CreateCommunity()
        {
            ErrorMessage = null;

            try
            {
                Community createdCommunity = new Community
                {
                    Name = _communityName,
                    Description = _communityDescription,
                    Picture = _communityPicture,
                    Banner = _communityBanner,
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