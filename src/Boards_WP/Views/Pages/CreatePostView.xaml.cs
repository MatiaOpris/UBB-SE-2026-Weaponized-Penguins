using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using System;

using Boards_WP.Data.Models;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreatePostView : Page
    {
        public static readonly DependencyProperty PostTitleProperty =
            DependencyProperty.Register("PostTitle", typeof(string), typeof(CreatePostView), new PropertyMetadata(string.Empty));

        public string PostTitle
        {
            get => (string)GetValue(PostTitleProperty);
            set => SetValue(PostTitleProperty, value);
        }

        public static readonly DependencyProperty PostDescriptionProperty =
            DependencyProperty.Register("PostDescription", typeof(string), typeof(CreatePostView), new PropertyMetadata(string.Empty));

        public string PostDescription
        {
            get => (string)GetValue(PostDescriptionProperty);
            set => SetValue(PostDescriptionProperty, value);
        }

        private Community _originCommunity;

        public CreatePostView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Community com)
            {
                _originCommunity = com;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void UploadPost_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PostTitle)) return;

            var newPost = new Post
            {
                PostID = new Random().Next(1000, 9999),
                Title = PostTitle,
                Description = PostDescription,
                ParentCommunity = _originCommunity,
                Owner = new User { Username = "@Me" },
                Score = 0,
                CommentsNumber = 0,
                CreationTime = DateTime.Now
            };

            this.Frame.Navigate(typeof(CommunityView), newPost);
        }
    }
}