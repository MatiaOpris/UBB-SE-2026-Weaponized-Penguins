using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IPostsService _postsService;
    private readonly UserSession _userSession;
    private readonly MainViewModel _mainViewModel;

    private int _currentOffset = 0;
    private const int PageSize = 200; //--PAGINATION

    private const int NoUserId = 0;

    [ObservableProperty]
    private bool _hasMorePosts = true;

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
        _currentOffset = 0;
        HasMorePosts = true;
        Posts.Clear();

        LoadBatch();
    }

    [RelayCommand]
    public void LoadBatch()
    {
        if (!HasMorePosts) return;

        var userId = _userSession.CurrentUser?.UserID ?? NoUserId;
        List<Post> data;

        if (IsHome)
            data = _postsService.GetPostsForHomePage(userId, _currentOffset, PageSize);
        else
            data = _postsService.GetPostsForDiscoveryPage(userId, _currentOffset, PageSize);

        if (data != null && data.Count > 0)
        {
            foreach (var post in data)
            {
                Posts.Add(new PostPreviewViewModel(post, _postsService, _userSession, _mainViewModel));
            }
            _currentOffset += data.Count;
        }

        HasMorePosts = (data?.Count == PageSize);
    }

    public void LoadHome() { IsHome = true; LoadFeed(); }
    public void LoadDiscovery() { IsHome = false; LoadFeed(); }

}