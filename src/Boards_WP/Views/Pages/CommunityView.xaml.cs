using System;
using System.Collections.ObjectModel;
using System.IO;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

using Boards_WP.Data.Models;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CommunityView : Page
    {
        public static readonly DependencyProperty CurrentCommunityProperty =
            DependencyProperty.Register("CurrentCommunity", typeof(Community), typeof(CommunityView), new PropertyMetadata(null));

        public Community CurrentCommunity
        {
            get => (Community)GetValue(CurrentCommunityProperty);
            set => SetValue(CurrentCommunityProperty, value);
        }

        public static readonly DependencyProperty IsMemberProperty =
            DependencyProperty.Register("IsMember", typeof(bool), typeof(CommunityView), new PropertyMetadata(false));

        public bool IsMember
        {
            get => (bool)GetValue(IsMemberProperty);
            set => SetValue(IsMemberProperty, value);
        }

        public ObservableCollection<Post> CommunityPosts { get; set; } = new();

        private static ObservableCollection<Community> _sidebarList;

        public BitmapImage BannerImage => ConvertToBitmap(CurrentCommunity?.Banner);
        public BitmapImage ProfileImage => ConvertToBitmap(CurrentCommunity?.Picture);
        public string MemberCountText => $"{CurrentCommunity?.MembersNumber ?? 0} members";
        public Visibility JoinButtonVisibility => IsMember ? Visibility.Collapsed : Visibility.Visible;
        public Visibility MemberActionsVisibility => IsMember ? Visibility.Visible : Visibility.Collapsed;

        public CommunityView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Community community)
            {
                CurrentCommunity = community;
                if (_sidebarList != null && _sidebarList.Contains(community))
                {
                    IsMember = true;
                }
                else if (community.Admin?.Username == "@Me")
                {
                    IsMember = true;
                }
                else
                {
                    IsMember = false;
                }
            }
            else if (e.Parameter is ObservableCollection<Community> list)
            {
                _sidebarList = list;
            }

            try { this.Bindings.Update(); } catch { }
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            IsMember = true;
            if (CurrentCommunity != null)
            {
                CurrentCommunity.MembersNumber++;
                if (_sidebarList != null && !_sidebarList.Contains(CurrentCommunity))
                    _sidebarList.Add(CurrentCommunity);
            }
            try { this.Bindings.Update(); } catch { }
        }

        private void LeaveButton_Click(object sender, RoutedEventArgs e)
        {
            IsMember = false;
            if (CurrentCommunity != null)
            {
                CurrentCommunity.MembersNumber--;
                _sidebarList?.Remove(CurrentCommunity);
            }
            try { this.Bindings.Update(); } catch { }
        }

        private void CreatePostButton_Click(object sender, RoutedEventArgs e) =>
            this.Frame.Navigate(typeof(CreatePostView), CurrentCommunity);

        private BitmapImage ConvertToBitmap(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var bitmap = new BitmapImage();
            using (var ms = new MemoryStream(data))
            {
                bitmap.SetSource(ms.AsRandomAccessStream());
            }
            return bitmap;
        }
    }
}