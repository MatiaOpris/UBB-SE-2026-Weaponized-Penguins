using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.ViewModels;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CommunityView : Page
    {
        public CommunityViewModel ViewModel { get; }

        public CommunityView()
        {
            this.InitializeComponent();

            var postsService = App.GetService<IPostsService>();

            ViewModel = new CommunityViewModel(
                navigateToCreatePost: community => Frame.Navigate(typeof(CreatePostView), community));

            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ApplyNavigationParameter(e.Parameter);
        }
    }
}