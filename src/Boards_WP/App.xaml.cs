using Microsoft.Extensions.DependencyInjection;
using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Boards_WP;

public partial class App : Application
{
    public Window? m_window;

    public new static App Current => (App)Application.Current;
    public static IServiceProvider Services { get; private set; } = null!;

    public static T GetService<T>() where T : class
        => Services.GetRequiredService<T>();

    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }


    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();

        var navService = Services.GetRequiredService<INavigationService>() as NavigationService;
        //if (m_window.Content is FrameworkElement root)
        //{
        //    var frame = root.FindName("ContentFrame") as Frame;
        //    if (frame != null)
        //    {
        //        navService.Initialize(frame);
        //        navService.NavigateTo(typeof(FeedView));
        //    }
        //}

        m_window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        //string connectionString = @"Server=ALEX\ALEXSQLEXPRESS;Database=Communities;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        //string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Communities;Trusted_Connection=True;TrustServerCertificate=True;";
        //string connectionString = @"Data Source=DESKTOP\SQLEXPRESS;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
        //string connectionString = @"Server=IONUT\SQLEXPRESS;Database=Communities;Trusted_Connection=True;TrustServerCertificate=True;";
        //services.AddSingleton<string>("Data Source=DESKTOP-GFA6UNJ\\SQLEXPRESS;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True");
        string connectionString = @"Data Source = DESKTOP\SQLEXPRESS;Database=Communities;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        services.AddSingleton(connectionString);


        services.AddSingleton<MainViewModel>();

        //--repos
        services.AddSingleton<IBetsRepository, BetsRepository>();
        services.AddSingleton<ICommentsRepository, CommentsRepository>();
        services.AddSingleton<ICommunitiesRepository, CommunitiesRepository>();
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IPostsRepository, PostsRepository>();
        services.AddSingleton<ITagsRepository, TagsRepository>();
        services.AddSingleton<IUsersMoodRepository, UsersMoodRepository>();
        services.AddSingleton<IUsersRepository, UsersRepository>();
        services.AddSingleton<IBetsRepository, BetsRepository>();

        // Services
        services.AddSingleton<IBetsService, BetsService>();
        services.AddSingleton<ICommentsService, CommentsService>();
        services.AddSingleton<ICommunitiesService, CommunitiesService>();
        services.AddSingleton<INotificationsService, NotificationsService>();
        services.AddSingleton<IPostsService, PostsService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IBetsService, BetsService>();
        services.AddTransient<LoginViewModel>();


        services.AddSingleton<UserSession>();

        // ViewModels
        services.AddSingleton<FeedViewModel>();
        services.AddTransient<NotificationItemViewModel>();
        services.AddTransient<NotificationsListViewModel>();
        services.AddTransient<CreatePostViewModel>();
        services.AddTransient<CommunityBarViewModel>();
        services.AddTransient<CreateCommunityViewModel>();
        services.AddTransient<UpdateCommunityViewModel>();
        services.AddSingleton<CommunityViewModel>();
        services.AddTransient<CreateTagViewModel>();
        services.AddTransient<CommentViewModel>();
        services.AddTransient<FullPostViewModel>();
        services.AddTransient<PostPreviewViewModel>();
        services.AddTransient<HeaderViewModel>();
        services.AddSingleton<CommunityBarViewModel>(); //--this must be signelton
        services.AddTransient<BetsViewModel>();

        return services.BuildServiceProvider();
    }
}