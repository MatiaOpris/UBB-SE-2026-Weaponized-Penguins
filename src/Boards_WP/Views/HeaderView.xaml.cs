using Microsoft.UI.Xaml.Controls;

namespace Boards_WP.Views
{
    // The "partial" keyword tells C# to combine this with the XAML file
    public sealed partial class HeaderView : UserControl
    {
        public HeaderView()
        {
            // This method is what actually draws the XAML on the screen
            this.InitializeComponent();
        }
    }
}