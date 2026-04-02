using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;
using Boards_WP.ViewModels;

namespace Boards_WP.Views.Pages
{
    public sealed partial class FullPostView : Page
    {
        public FullPostViewModel ViewModel { get; }

        public FullPostView()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<FullPostViewModel>();
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.IsCommentAreaVisible) && ViewModel.IsCommentAreaVisible)
                {
                    CommentInput.Focus(FocusState.Programmatic);
                }
            };
        }

        public static Visibility NullToVisibility(byte[] value) =>
            value != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility BooleanToVisibility(bool value) =>
            value ? Visibility.Visible : Visibility.Collapsed;

        public Visibility BooleanToInverseVisibility(bool value) =>
            value ? Visibility.Collapsed : Visibility.Visible;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Post clickedPost)
            {
                ViewModel.Initialize(clickedPost);
                this.Bindings.Update();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}