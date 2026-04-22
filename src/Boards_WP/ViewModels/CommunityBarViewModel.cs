using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;

using CommunityToolkit.Mvvm.Input;

public partial class CommunityBarViewModel
{
    private readonly INavigationService navigationService;
    private readonly ICommunitiesService communitiesService;
    private readonly FeedViewModel feedViewModel;
    private readonly MainViewModel mainViewModel;
    private readonly UserSession userSession;

    public MainViewModel MainViewModel => mainViewModel;

    public ObservableCollection<Community> Communities { get; } = new ();

    public CommunityBarViewModel(
        INavigationService navigationService, ICommunitiesService communitiesService,
        FeedViewModel feedViewModel, UserSession userSession, MainViewModel mainViewModel)
    {
        this.navigationService = navigationService;
        this.communitiesService = communitiesService;
        this.feedViewModel = feedViewModel;
        this.userSession = userSession;
        this.mainViewModel = mainViewModel;

        LoadCommunities();
    }

    public void LoadCommunities()
    {
        Communities.Clear();
        if (userSession?.CurrentUser == null)
        {
            return;
        }

        foreach (Community community in communitiesService.GetCommunitiesUserIsPartOf(userSession.CurrentUser.UserID))
        {
            Communities.Add(community);
        }
    }

    [RelayCommand]
    private void NavigateHome()
    {
        feedViewModel.IsHome = true;
        feedViewModel.LoadHome();
        navigationService.NavigateTo(typeof(FeedView));
    }

    [RelayCommand]
    private void NavigateDiscovery()
    {
        feedViewModel.IsHome = false;
        feedViewModel.LoadDiscovery();
        navigationService.NavigateTo(typeof(FeedView));
    }

    [RelayCommand]
    private void NavigateCreateCommunity()
    {
        navigationService.NavigateTo(typeof(CreateCommunityView));
    }

    [RelayCommand]
    private void NavigateToCommunity(Community community)
    {
        if (community != null)
        {
            navigationService.NavigateTo(typeof(CommunityView), community);
        }
    }
}