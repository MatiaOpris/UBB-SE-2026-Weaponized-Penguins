using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;

namespace Boards_WP.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IPostsService _postsService;
    private readonly UserSession _userSession;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private bool _isHome = true;
    public ObservableCollection<PostPreviewViewModel> Posts { get; } = new();

    public FeedViewModel(IPostsService postsService, UserSession userSession, MainViewModel mainViewModel)
    {
        _postsService = postsService;
        _userSession = userSession;
        _mainViewModel = mainViewModel;
    }

    public void LoadFeed()
    {
        if (_isHome)
            LoadHome();
        else
            LoadDiscovery();
    }
    public void LoadHome()
    {
        
        Posts.Clear();

        // Fetch data from service
        var userId = _userSession.CurrentUser?.UserID ?? 0;
        var data = _postsService.GetPostsForHomePage(userId);

        // IMPORTANT: Add each post to the ObservableCollection
        foreach (var post in data)
        {
            var previewVm = new PostPreviewViewModel(post, _postsService, _userSession, _mainViewModel);
            Posts.Add(previewVm);
        }
    }

    public void LoadDiscovery()
    {

        Posts.Clear();
        var userId = _userSession.CurrentUser?.UserID ?? 0;

        // Fetch raw database models
        var rawPosts = _postsService.GetPostsForDiscoveryPage(userId);

        // Wrap them in ViewModels before passing them to the UI!
        foreach (var post in rawPosts)
        {
            var previewVm = new PostPreviewViewModel(post, _postsService, _userSession, _mainViewModel);
            Posts.Add(previewVm);
        }
    }
}