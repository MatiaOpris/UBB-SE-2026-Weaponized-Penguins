using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Boards_WP.Data.Models;
using Boards_WP.Data.Services.Interfaces;
using System;

namespace Boards_WP.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Brush _appThemeBrush;

        [ObservableProperty]
        private Brush _vividThemeBrush;

        private readonly Windows.UI.Color _defaultColor = Windows.UI.Color.FromArgb(255, 230, 226, 255);
        private readonly Windows.UI.Color _defaultVividColor = Windows.UI.Color.FromArgb(255, 120, 90, 200);

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            AppThemeBrush = new SolidColorBrush(_defaultColor);
            VividThemeBrush = new SolidColorBrush(_defaultVividColor);
        }

        public void InitializeNavigation(object frame)
        {
            _navigationService.Initialize(frame);
            _navigationService.NavigateTo(typeof(Views.Pages.FeedView));
        }

        public void ApplyNewTheme(ThemeColor newTheme)
        {
            if (newTheme == ThemeColor.Default)
            {
                AppThemeBrush = new SolidColorBrush(_defaultColor);
                VividThemeBrush = new SolidColorBrush(_defaultVividColor);
                return;
            }

            Windows.UI.Color actualColor = MapEnumToUiColor(newTheme);

            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1)
            };

            gradientBrush.GradientStops.Add(new GradientStop { Color = actualColor, Offset = 0.0 });
            gradientBrush.GradientStops.Add(new GradientStop { Color = actualColor, Offset = 0.7 });
            gradientBrush.GradientStops.Add(new GradientStop { Color = _defaultColor, Offset = 1.0 });

            AppThemeBrush = gradientBrush;

            Windows.UI.Color vividColor = MapEnumToVividColor(newTheme);
            VividThemeBrush = new SolidColorBrush(vividColor);
        }

        private Windows.UI.Color MapEnumToUiColor(ThemeColor colorEnum)
        {
            return colorEnum switch
            {
                ThemeColor.Pink => Windows.UI.Color.FromArgb(255, 255, 192, 203),
                ThemeColor.Orange => Windows.UI.Color.FromArgb(255, 255, 200, 120),
                ThemeColor.Turquoise => Windows.UI.Color.FromArgb(255, 175, 238, 238),
                ThemeColor.Yellow => Windows.UI.Color.FromArgb(255, 255, 255, 153),
                ThemeColor.Blue => Windows.UI.Color.FromArgb(255, 173, 216, 230),
                ThemeColor.Green => Windows.UI.Color.FromArgb(255, 144, 238, 144),
                ThemeColor.Red => Windows.UI.Color.FromArgb(255, 255, 182, 193),
                ThemeColor.Purple => Windows.UI.Color.FromArgb(255, 216, 191, 216),
                _ => _defaultColor
            };
        }

        private Windows.UI.Color MapEnumToVividColor(ThemeColor colorEnum)
        {
            return colorEnum switch
            {
                ThemeColor.Pink => Windows.UI.Color.FromArgb(255, 255, 20, 147),
                ThemeColor.Orange => Windows.UI.Color.FromArgb(255, 255, 140, 0),
                ThemeColor.Turquoise => Windows.UI.Color.FromArgb(255, 0, 139, 139),
                ThemeColor.Yellow => Windows.UI.Color.FromArgb(255, 218, 165, 32),
                ThemeColor.Blue => Windows.UI.Color.FromArgb(255, 0, 0, 255),
                ThemeColor.Green => Windows.UI.Color.FromArgb(255, 0, 128, 0),
                ThemeColor.Red => Windows.UI.Color.FromArgb(255, 220, 20, 60),
                ThemeColor.Purple => Windows.UI.Color.FromArgb(255, 128, 0, 128),
                _ => _defaultVividColor
            };
        }
    }
}