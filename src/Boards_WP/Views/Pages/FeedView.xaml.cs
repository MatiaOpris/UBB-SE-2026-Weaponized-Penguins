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
                    PostID = 1,
                    Title = "How to make your CV better",
                    Owner = new User { Username = "@AlexBindiu" },
                    Community = new Community { Name = "Computer Science" },
                    Score = 50,
                    CommentsNumber = 30,
                    CreationTime = new DateTime(2026, 3, 11),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                },
                new Post {
                    PostID = 2,
                    Title = ".NET Internship",
                    Owner = new User { Username = "@Matei" },
                    Community = new Community { Name = "UBB" },
                    Score = 37,
                    CommentsNumber = 9,
                    CreationTime = new DateTime(2026, 3, 7),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                },
                new Post {
                    PostID = 3,
                    Title = "My First Meme!",
                    Owner = new User { Username = "@AlexandraBochis" },
                    Community = new Community { Name = "WeaponizedPenguins" },
                    Score = 17,
                    CommentsNumber = 5,
                    CreationTime = new DateTime(2026, 2, 28),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                }
            };

            FeedList.ItemsSource = posts;
        }
    }
}