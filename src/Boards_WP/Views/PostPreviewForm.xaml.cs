using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views
{
    public sealed partial class PostPreviewForm : UserControl
    {
        public ViewModels.PostPreviewViewModel ViewModel => DataContext as ViewModels.PostPreviewViewModel;

        public PostPreviewForm()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                Bindings.Update();
            };
        }
    }
}