using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FullPostView : Page
    {
        public FullPostView()
        {
            this.InitializeComponent();
        }


        // we use this function to display the preview form of the clicked post (so that when we enter full view post, the preview form gets shown at the top, above the comments)
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // checking if the data we were given is actually an object of type Post, if yes, we display its preview form 
            if (e.Parameter is Post selectedPost)
            {
                FullPostHeader.PostData = selectedPost;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // checking if there is a page to go back to, if there is, then we go back to it
            // CanGoBack is a built-in property that remembers where the user was
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                // if we dont have a previous page to go back to, we tell the app to go back to the FeedView
                this.Frame.Navigate(typeof(FeedView));
            }
        }
    }
}