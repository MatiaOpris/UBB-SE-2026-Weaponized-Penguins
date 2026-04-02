using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Windows.Networking.NetworkOperators;

namespace Boards_WP.ViewModels;


public partial class HeaderViewModel : ObservableObject
{
    private readonly ICommunitiesService _communitiesService;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private readonly UserSession _userSession;

    public MainViewModel MainViewModel => _mainViewModel;

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
    }

    partial void OnSearchTextChanged(string searchedValue)
    {
        System.Diagnostics.Debug.WriteLine($"Searching for: {searchedValue}");

        if (string.IsNullOrWhiteSpace(searchedValue) || searchedValue.Length < 2)
        {
            SearchResults.Clear();
            NoResultsToggle = false;
            return;
        }

        var matches = _communitiesService.searchCommunities(searchedValue);

        System.Diagnostics.Debug.WriteLine($"Found {matches.Count} matches");

        SearchResults.Clear();
        foreach (var community in matches)
        {
            SearchResults.Add(community);
        }

        NoResultsToggle = (SearchResults.Count == 0);
    }

    [RelayCommand]
    public void SelectCommunity(Community selected)
    {
        if (selected == null) return;

        SearchText = string.Empty;
        SearchResults.Clear();

        if (App.Current is App myApp && myApp.m_window is MainWindow mainWindow)
        {
            _navigationService.NavigateTo(typeof(Views.Pages.CommunityView), selected);
        }
    }
}