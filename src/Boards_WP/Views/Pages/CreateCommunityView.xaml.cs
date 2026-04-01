using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using System.Collections.ObjectModel;

using Boards_WP.Data.Models;
using Boards_WP.ViewModels;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreateCommunityView : Page
    {
        public CreateCommunityViewModel ViewModel { get; }

        public CreateCommunityView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<CreateCommunityViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Community> list)
                ViewModel.SidebarList = list;
        }
    }
}