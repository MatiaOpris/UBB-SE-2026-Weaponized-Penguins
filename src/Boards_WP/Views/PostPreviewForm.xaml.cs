using Boards_WP.Data.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;

namespace Boards_WP.Views
{
    public sealed partial class PostPreviewForm : UserControl
    {
        // DependencyProperty allows the ListView to pass data into this control
        public static readonly DependencyProperty PostDataProperty =
            DependencyProperty.Register("PostData", typeof(Post), typeof(PostPreviewForm), new PropertyMetadata(null));


        // we declare an object called "PostData" of type Post, which we will use when the user interacts with one of the posts in preview form
        // we use this object to ensure if for example, the user likes a post, the score won't just change on screen, but will actually modify the score of the post
        public Post PostData
        {
            get => (Post)GetValue(PostDataProperty);
            set => SetValue(PostDataProperty, value);
        }

        public PostPreviewForm()
        {
            this.InitializeComponent();
        }

        
        public string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        private void Upvote_Click(object sender, RoutedEventArgs e)
        {
            if (PostData == null) return;
            PostData.Score++;
            ScoreLabel.Text = PostData.Score.ToString();
        }

        private void Downvote_Click(object sender, RoutedEventArgs e)
        {
            if (PostData == null) return;
            PostData.Score--;
            ScoreLabel.Text = PostData.Score.ToString();
        }
    }
}