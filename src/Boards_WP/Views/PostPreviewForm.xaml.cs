using Boards_WP.Data.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;

using Boards_WP.Data.Models;

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
        // PostData allows us to communicate with the data we have and display it on screen properly
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


        // this is the function which opens the clicked post, from preview form to full view form
        // it tells the MainWindow to change the Frame/"main section" to the FullView of the clicke post
        private void OnPostClicked(object sender, PointerRoutedEventArgs e)
        {
            if (PostData == null) return;

            // we cast App.Current to our specific App class to access the public m_window
            if (App.Current is App myApp && myApp.m_window is MainWindow mainWindow)
            {
                mainWindow.NavigateToPage(typeof(Pages.FullPostView), this.PostData);
            }
        }

        // helper to find the Frame without protection level errors
        private T FindChildElementByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T element && (element as FrameworkElement)?.Name == name) return element;
                var result = FindChildElementByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }


        // this function is responsible with incrementing the score of the post
        private void Upvote_Click(object sender, RoutedEventArgs e)
        {
            if (PostData == null) return;
            PostData.Score++;
            ScoreLabel.Text = PostData.Score.ToString();
        }

        // this function is responsible with decrementing the score of the post
        private void Downvote_Click(object sender, RoutedEventArgs e)
        {
            if (PostData == null) return;
            PostData.Score--;
            ScoreLabel.Text = PostData.Score.ToString();
        }


        // this function changes the cursor from arrow to hand when the mouse enters the post card
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        // this function changes the cursor from hand to arrow when the mouse leaves the post card
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }


    }
}