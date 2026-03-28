using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Models;

using System.Collections.Generic;
using System;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FeedView : Page
    {
        public FeedView()
        {
            this.InitializeComponent();

            var posts = new List<Post>
            {
                new Post {
                    Title = "How to make your CV better",
                    OwnerName = "@AlexBindiu",
                    CommunityName = "Computer Science",
                    Score = 50,
                    CommentsNumber = 30,
                    CreationTime = new DateTime(2026, 3, 11),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
                },
                new Post {
                    Title = ".NET Internship",
                    OwnerName = "@Matei",
                    CommunityName = "UBB",
                    Score = 37,
                    CommentsNumber = 9,
                    CreationTime = new DateTime(2026, 3, 7),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
                },
                new Post {
                    Title = "My First Meme!",
                    OwnerName = "@AlexandraBochis",
                    CommunityName = "WeaponizedPenguins",
                    Score = 17,
                    CommentsNumber = 5,
                    CreationTime = new DateTime(2026, 2, 28),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
                }
            };

            FeedList.ItemsSource = posts;
        }
    }
}