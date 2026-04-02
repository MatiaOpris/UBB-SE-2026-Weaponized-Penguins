using Boards_WP.Data.Services.Interfaces;


using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CommunityView : Page
    {
        public CommunityViewModel ViewModel { get; }

        public CommunityView() 
        {
            this.InitializeComponent();
            ViewModel = new CommunityViewModel(
                navigateToCreatePost: community => Frame.Navigate(typeof(CreatePostView), community),
                navigateToEditCommunity: community => Frame.Navigate(typeof(UpdateCommunityView), community));

            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ApplyNavigationParameter(e.Parameter);
        }
    }
}