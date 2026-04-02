using Boards_WP.ViewModels; // Adjust to your actual VM namespace

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Boards_WP.Views
{
    public sealed partial class CommunityBarView : UserControl
    {
        public ObservableCollection<Community> Communities { get; set; } = new();
        public CommunityBarViewModel ViewModel { get; private set; }

        public CommunityBarView()
        {
            this.ViewModel = App.GetService<CommunityBarViewModel>();
            //to do - load communities from service
            this.InitializeComponent();
            this.Loaded += CommunityBarView_Loaded;
        }
        private void CommunityBarView_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel = App.GetService<CommunityBarViewModel>();
            this.DataContext = this.ViewModel;
            this.Bindings.Update();
        }

        private void CommunityListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Community selected)
            {
                ViewModel.NavigateToCommunityCommand.Execute(selected);
            }
        }

        // We can now remove the complex FindChildFrame and manual navigation logic 
        // for Home and Discover because the ViewModel handles them via RelayCommands!

        private void HomeNavigation_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.NavigateHomeCommand.Execute(null);
        }

        private void DiscoverNavigation_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.NavigateDiscoveryCommand.Execute(null);
        }

        private void StartCommunity_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.NavigateCreateCommunityCommand.Execute(null);
        }
    }
}