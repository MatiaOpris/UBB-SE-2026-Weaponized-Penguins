using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Boards_WP.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IPostsService postsService;
    private readonly UserSession userSession;
    private readonly MainViewModel mainViewModel;

    private int currentOffset = 0;
    private const int PageSize = 200; //--PAGINATION

    [ObservableProperty]
    private bool hasMorePosts = true;

    [ObservableProperty]
    private bool isHome = true;
    public ObservableCollection<PostPreviewViewModel> Posts { get; } = new ();

    public FeedViewModel(IPostsService postsService, UserSession userSession, MainViewModel mainViewModel)
    {
        this.postsService = postsService;
        this.userSession = userSession;
        this.mainViewModel = mainViewModel;
    }

    public void LoadFeed()
    {
        currentOffset = 0;
        HasMorePosts = true;
        Posts.Clear();

        LoadBatch();
    }

    [RelayCommand]
    public void LoadBatch()
    {
        if (!HasMorePosts)
        {
            return;
        }

        var userId = userSession.CurrentUser?.UserID ?? 0;
        List<Post> data;

        if (IsHome)
        {
            data = postsService.GetPostsForHomePage(userId, currentOffset, PageSize);
        }
        else
        {
            data = postsService.GetPostsForDiscoveryPage(userId, currentOffset, PageSize);
        }

        if (data != null && data.Count > 0)
        {
            foreach (var post in data)
            {
                Posts.Add(new PostPreviewViewModel(post, postsService, userSession, mainViewModel));
            }
            currentOffset += data.Count;
        }

        HasMorePosts = (data?.Count == PageSize);
    }

    public void LoadHome()
    {
        IsHome = true;
        LoadFeed();
    }
    public void LoadDiscovery()
    {
        IsHome = false;
        LoadFeed();
    }
}