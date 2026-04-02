using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services; // Ensure this matches your Service namespace
using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;
using Boards_WP;

using CommunityToolkit.Mvvm.Input;

public partial class CommunityBarViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ICommunitiesService _communitiesService;
    private readonly FeedViewModel _feedViewModel; // Injected Singleton
    private readonly MainViewModel _mainViewModel;
    private readonly UserSession _userSession;

    public MainViewModel MainViewModel => _mainViewModel;

    public ObservableCollection<Community> Communities { get; } = new();

    public CommunityBarViewModel(
        INavigationService navigationService, ICommunitiesService communitiesService,
        FeedViewModel feedViewModel, UserSession userSession, MainViewModel mainViewModel)
    {
        _navigationService = navigationService;
        _communitiesService = communitiesService;
        _feedViewModel = feedViewModel;
        _userSession = userSession;
        _mainViewModel = mainViewModel;

        LoadCommunities();
    }

    public void LoadCommunities()
    {
        Communities.Clear();
        if (_userSession?.CurrentUser == null) return;

        foreach (Community community in _communitiesService.GetCommunitiesUserIsPartOf(_userSession.CurrentUser.UserID))
            Communities.Add(community);
    }

    [RelayCommand]
    private void NavigateHome()
    {
        _feedViewModel.LoadHome(); 
        _navigationService.NavigateTo(typeof(FeedView));
    }

    [RelayCommand]
    private void NavigateDiscovery()
    {
        _feedViewModel.LoadDiscovery();
        _navigationService.NavigateTo(typeof(FeedView));
    }

    [RelayCommand]
    private void NavigateCreateCommunity()
    {
        _navigationService.NavigateTo(typeof(CreateCommunityView));
    }

    [RelayCommand]
    private void NavigateToCommunity(Community community)
    {
        if (community != null)
        {
            _navigationService.NavigateTo(typeof(CommunityView), community);
        }
    }
}