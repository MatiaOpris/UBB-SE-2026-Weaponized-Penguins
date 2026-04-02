using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FullPostView : Page
    {
        public ViewModels.FullPostViewModel ViewModel { get; }

        public FullPostView()
        {
            this.InitializeComponent();

            // Resolve ViewModel from the DI container (keeps the constructor parameterless for XAML)
            ViewModel = App.Services.GetRequiredService<ViewModels.FullPostViewModel>();
        }

        public static Microsoft.UI.Xaml.Visibility NullToVisibility(byte[] value)
        {
            return value != null ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Post clickedPost)
            {
                ViewModel.Initialize(clickedPost);
                this.Bindings.Update(); // This ensures the UI renders the data immediately
            }
        }

        // Note: Frame manipulation is purely UI-level logic, so it is acceptable 
        // to leave this in the code-behind unless you are using a dedicated INavigationService.
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}