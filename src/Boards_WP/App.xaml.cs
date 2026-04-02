using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Boards_WP.Data.Services;
using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;

using Microsoft.Extensions.DependencyInjection;
﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Services.Interfaces;
using Boards_WP.Data.Services;
using Boards_WP.ViewModels;

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
        InitializeComponent();

        Services = new ServiceCollection()

            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<UserSession>()

            .AddSingleton<IPostsService, PostsService>()
            .AddSingleton<ICommunitiesService, CommunitiesService>()
            .AddSingleton<INotificationsService, NotificationsService>()
            .AddSingleton<ICommentsService, CommentsService>()
            .AddSingleton<IBetsService, BetsService>()
            .AddSingleton<IUsersService, UsersService>()

            .AddSingleton<string>("Data Source=IONUT\\SQLEXPRESS;Initial Catalog=Communities;Integrated Security=True;Encrypt=False;TrustServerCertificate=True")
            .AddSingleton<IPostsRepository, PostsRepository>()
            .AddSingleton<ICommunitiesRepository, CommunitiesRepository>()
            .AddSingleton<INotificationRepository, NotificationRepository>()
            .AddSingleton<ICommentsRepository, CommentsRepository>()
            .AddSingleton<IBetsRepository, BetsRepository>()
            .AddSingleton<IUsersRepository, UsersRepository>()
            .AddSingleton<ITagsRepository, TagsRepository>()
            .AddSingleton<IUsersMoodRepository, UsersMoodRepository>()

            .AddSingleton<FeedViewModel>()
            //VMs
            .AddTransient<CommunityViewModel>()
            .AddTransient<CreateCommunityViewModel>()
            //.AddTransient<CreatePostViewModel>()
            .AddTransient<FullPostViewModel>()
            .AddTransient<NotificationItemViewModel>()
            .AddTransient<NotificationsListViewModel>()
            .AddTransient<MainViewModel>()

            // Components / UserControls
            .AddTransient<PostPreviewViewModel>()
            .AddTransient<CommentViewModel>()
            .AddTransient<CommunityBarViewModel>()
            //.AddTransient<HeaderViewModel>()

            .BuildServiceProvider();
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


}