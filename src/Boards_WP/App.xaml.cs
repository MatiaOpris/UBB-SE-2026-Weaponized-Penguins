using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Boards_WP.Data.Services;

using Microsoft.Extensions.DependencyInjection;
using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Data.Services;

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
        if (m_window.Content is FrameworkElement root)
        {
            var frame = root.FindName("ContentFrame") as Frame;
            if (frame != null)
            {
                navService.Initialize(frame);
                navService.NavigateTo(typeof(FeedView));
            }
        }

        m_window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

string connectionString = @"Data Source=DESKTOP-1JCJMN6\SQLEXPRESS;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
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

        // Services
        services.AddSingleton<IBetsService, BetsService>();
        services.AddSingleton<ICommentsService, CommentsService>();
        services.AddSingleton<ICommunitiesService, CommunitiesService>();
        services.AddSingleton<INotificationsService, NotificationsService>();
        services.AddSingleton<IPostsService, PostsService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<UserSession>();

        // ViewModels
        services.AddSingleton<FeedViewModel>();
        services.AddTransient<NotificationItemViewModel>();
        services.AddTransient<NotificationsListViewModel>();
        services.AddTransient<CreatePostViewModel>();
        services.AddTransient<CommunityBarViewModel>();
        services.AddTransient<CreateCommunityViewModel>();
        services.AddTransient<UpdateCommunityViewModel>();
        services.AddTransient<CommunityViewModel>();
        services.AddTransient<CreateTagViewModel>();
        services.AddTransient<CommentViewModel>();
        services.AddTransient<FullPostViewModel>();
        services.AddTransient<PostPreviewViewModel>();
        services.AddTransient<NotificationItemViewModel>();
        services.AddTransient<NotificationsListViewModel>();
        services.AddTransient<HeaderViewModel>();
        services.AddTransient<CommunityBarViewModel>();
        services.AddTransient<BetsViewModel>();

        return services.BuildServiceProvider();
    }
}