using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.ViewModels;

namespace Boards_WP.Views
{
    public sealed partial class BetItemControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(BetItemViewModel), typeof(BetItemControl), new PropertyMetadata(null));

        

        public BetItemViewModel ViewModel
        {
            get => (BetItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public BetItemControl()
        {
            this.InitializeComponent();
        }
    }
}