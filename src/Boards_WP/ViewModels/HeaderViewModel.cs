using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;

namespace Boards_WP.ViewModels
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly ICommunitiesService _communitiesService;
        private readonly INavigationService _navigationService;
        private readonly UserSession _userSession;
        private readonly MainViewModel _mainViewModel;

        public MainViewModel MainViewModel => _mainViewModel;


        [ObservableProperty]
        private int _userTokens;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Community> _searchResults = new();

        [ObservableProperty]
        private bool _noResultsToggle;

        public HeaderViewModel(ICommunitiesService communitiesService, INavigationService navigationService, UserSession userSession, MainViewModel mainViewModel)
        {
            _communitiesService = communitiesService;
            _navigationService = navigationService;
            _userSession = userSession;
            _mainViewModel = mainViewModel;


            UserTokens = 1250;
        }

        
        partial void OnSearchTextChanged(string searchedValue)
        {
            if (string.IsNullOrWhiteSpace(searchedValue))
            {
                SearchResults.Clear();
                NoResultsToggle = false;
                return;
            }

            
            var matches = _communitiesService.searchCommunities(searchedValue);

            SearchResults.Clear();
            foreach (var community in matches)
            {
                SearchResults.Add(community);
            }

            if (SearchResults.Count == 0)
            {
                SearchResults.Add(new Community { CommunityID = -1, Name = "no results are found" });
            }

            NoResultsToggle = (matches.Count == 0);
        }

        [RelayCommand]
        public void SelectCommunity(Community selected)
        {
            if (selected == null) return;

            SearchText = string.Empty;
            SearchResults.Clear();

            
            _navigationService.NavigateTo(typeof(Views.Pages.CommunityView), selected);
        }
    }
}