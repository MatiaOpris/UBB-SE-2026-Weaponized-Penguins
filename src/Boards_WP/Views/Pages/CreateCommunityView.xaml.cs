using System.Runtime.InteropServices;

using Boards_WP.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.Storage.Pickers;

using WinRT.Interop;

namespace Boards_WP.Views.Pages
{
    public sealed partial class CreateCommunityView : Page
    {
        public CreateCommunityViewModel ViewModel { get; }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public CreateCommunityView()
        {
            ViewModel = App.GetService<CreateCommunityViewModel>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Community> list)
            {
                ViewModel.SidebarList = list;
            }
        }

        private async void PicturePicker_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            byte[] bytes = await PickImageBytesAsync();
            if (bytes is null)
            {
                return;
            }

            ViewModel.CommunityPicture = bytes;

            using var ms = new MemoryStream(bytes);
            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(ms.AsRandomAccessStream());
            PicturePreviewBrush.ImageSource = bmp;
            PicturePreviewMask.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PictureIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private async void BannerPicker_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            byte[] bytes = await PickImageBytesAsync();
            if (bytes is null)
            {
                return;
            }

            ViewModel.CommunityBanner = bytes;

            using var ms = new MemoryStream(bytes);
            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(ms.AsRandomAccessStream());
            BannerPreview.Source = bmp;
        }

        private async Task<byte[]> PickImageBytesAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");

            InitializeWithWindow.Initialize(picker, GetActiveWindow());

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return null;
            }

            using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.AsStreamForRead().CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
