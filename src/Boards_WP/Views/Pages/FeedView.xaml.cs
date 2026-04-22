using Boards_WP.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FeedView : Page
    {
        public FeedViewModel? ViewModel { get; set; }

        public FeedView()
        {
            ViewModel = App.Services?.GetService<FeedViewModel>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadFeed();
        }

        public Visibility GetVisibility(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;
    }
}