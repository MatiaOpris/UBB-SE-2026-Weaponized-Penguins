using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace Boards_WP.Views
{
    public sealed partial class PostPreviewForm : UserControl
    {
        // Expose the DataContext as your specific ViewModel for x:Bind
        public ViewModels.PostPreviewViewModel ViewModel => DataContext as ViewModels.PostPreviewViewModel;

        public PostPreviewForm()
        {
            this.InitializeComponent();

            // When the ListView hands this UserControl a new ViewModel, tell the bindings to update
            this.DataContextChanged += (s, e) =>
            {
                Bindings.Update();
            };
        }
    }
}