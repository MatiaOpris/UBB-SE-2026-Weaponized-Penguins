using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;
using Boards_WP.ViewModels;
using Microsoft.UI.Xaml;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreatePostView : Page
    {
        public CreatePostViewModel ViewModel { get; }

        public CreatePostView()
        {
            ViewModel = App.GetService<CreatePostViewModel>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Community com)
            {
                ViewModel.OriginCommunity = com;
            }   
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}