using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Boards_WP.ViewModels
{
    public partial class UpdateCommunityViewModel : ObservableObject
    {
        private readonly ICommunitiesService _communitiesService;
        private readonly INavigationService _navigationService;
        private readonly MainViewModel _mainViewModel;
        private readonly UserSession _userSession;

        private Community _community = null!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommunityCommand))]
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

        public UpdateCommunityViewModel()
        {
            _mainViewModel = App.Services?.GetService<MainViewModel>();
            _communitiesService = App.Services?.GetService<ICommunitiesService>();
            _navigationService = App.Services?.GetService<INavigationService>();
            _userSession = App.Services?.GetService<UserSession>();
        }

        public void Initialize(Community community)
        {
            _community = community;

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
                _community.Name = CommunityName;
                _community.Description = CommunityDescription;
                _community.Picture = CommunityPicture;
                _community.Banner = CommunityBanner;

                _communitiesService.UpdateCommunityInfo(_community.CommunityID, CommunityDescription, CommunityPicture, CommunityBanner);

                _navigationService.NavigateTo(typeof(CommunityView), _community);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void Cancel() => _navigationService.GoBack();

        private bool CanUpdateCommunity() => !string.IsNullOrWhiteSpace(CommunityName);
    }
}