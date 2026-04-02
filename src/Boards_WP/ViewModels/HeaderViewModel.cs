using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;

namespace Boards_WP.ViewModels;


public partial class HeaderViewModel : ObservableObject
{
    private readonly ICommunitiesService _communitiesService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Community> _searchResults = new();

    [ObservableProperty]
    private bool _noResultsToggle;

    public HeaderViewModel(ICommunitiesService communitiesService, INavigationService navigationService)
    {
        _communitiesService = communitiesService;
        _navigationService = navigationService;
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