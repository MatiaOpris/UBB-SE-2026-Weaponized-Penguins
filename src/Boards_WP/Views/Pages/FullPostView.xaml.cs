using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;
using Boards_WP.ViewModels;

using System;
using System.Collections.Generic;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FullPostView : Page
    {
        public FullPostViewModel ViewModel { get; } = new FullPostViewModel();

        public FullPostView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Post clickedPost)
            {
                ViewModel.CurrentPost = clickedPost;
                LoadMockComments();
            }
        }

        private void LoadMockComments()
        {
            var hardcodedComments = new List<Comment>
            {
                new Comment { CommentID = 1, Owner = new User { Username = "@Alexandra" }, Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ", Score = 15, CreationTime = DateTime.Now.AddHours(-2), Indentation = 0 },
                new Comment { CommentID = 2, Owner = new User { Username = "@BerceaMatei" }, Description = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", Score = 8, CreationTime = DateTime.Now.AddHours(-1), Indentation = 1 },
                new Comment { CommentID = 3, Owner = new User { Username = "@RazvanBerbecar" }, Description = "Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?", Score = 12, CreationTime = DateTime.Now.AddMinutes(-30), Indentation = 2 },
                new Comment { CommentID = 4, Owner = new User { Username = "@BeneIonut" }, Description = "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. ", Score = 2, CreationTime = DateTime.Now.AddMinutes(-10), Indentation = 0 }
            };

            ViewModel.PostComments.Clear();
            foreach (var c in hardcodedComments)
            {
                ViewModel.PostComments.Add(c);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }
    }
}