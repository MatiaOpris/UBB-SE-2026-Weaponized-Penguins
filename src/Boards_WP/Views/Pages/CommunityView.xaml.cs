using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Services.Interfaces;
using Boards_WP.ViewModels;
using Boards_WP.Views.Pages;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CommunityView : Page
    {
        public CommunityViewModel ViewModel { get; }

        public CommunityView(IPostsService postsService)
        {
            this.InitializeComponent();
            ViewModel = new CommunityViewModel(
                navigateToCreatePost: community => Frame.Navigate(typeof(CreatePostView), community));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ApplyNavigationParameter(e.Parameter);
        }
    }
}