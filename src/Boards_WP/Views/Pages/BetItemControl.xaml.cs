using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Boards_WP.Data.Models;

namespace Boards_WP.Views
{
    public sealed partial class BetItemControl : UserControl
    {
        public static readonly DependencyProperty BetDataProperty =
            DependencyProperty.Register("BetData", typeof(Bet), typeof(BetItemControl), new PropertyMetadata(null));

        public Bet BetData
        {
            get => (Bet)GetValue(BetDataProperty);
            set => SetValue(BetDataProperty, value);
        }

        public BetItemControl()
        {
            this.InitializeComponent();
        }
    }
}