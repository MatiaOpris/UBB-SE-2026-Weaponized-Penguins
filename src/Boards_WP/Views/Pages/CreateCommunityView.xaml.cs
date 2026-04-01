using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using System.Collections.ObjectModel;

using Boards_WP.Data.Models;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreateCommunityView : Page
    {
        public static readonly DependencyProperty CommunityNameProperty =
            DependencyProperty.Register("CommunityName", typeof(string), typeof(CreateCommunityView), new PropertyMetadata(string.Empty));

        public string CommunityName
        {
            get => (string)GetValue(CommunityNameProperty);
            set => SetValue(CommunityNameProperty, value);
        }

        public static readonly DependencyProperty CommunityDescriptionProperty =
            DependencyProperty.Register("CommunityDescription", typeof(string), typeof(CreateCommunityView), new PropertyMetadata(string.Empty));

        public string CommunityDescription
        {
            get => (string)GetValue(CommunityDescriptionProperty);
            set => SetValue(CommunityDescriptionProperty, value);
        }

        private ObservableCollection<Community> _sidebarList;

        public CreateCommunityView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Community> list)
            {
                _sidebarList = list;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void CreateCommunity_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommunityName)) return;

            var newCommunity = new Community
            {
                CommunityID = 99,
                Name = CommunityName,
                Description = CommunityDescription,
                Admin = new User { Username = "@Me" },
                MembersNumber = 1
            };

            _sidebarList?.Add(newCommunity);
            this.Frame.Navigate(typeof(CommunityView), newCommunity);
        }
    }
}