using Microsoft.UI.Xaml.Controls;
namespace Boards_WP.Data.Services;

public class NavigationService : INavigationService
{
    private Frame? frame;

    public bool CanGoBack => frame?.CanGoBack ?? false;

    public void Initialize(object frame)
    {
        this.frame = frame as Frame;
    }

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        if (frame == null)
        {
            throw new InvalidOperationException("NavigationService is not initialized. Call Initialize with a Frame before navigating.");
        }

        frame.Navigate(pageType, parameter);
    }
    public void GoBack()
    {
        if (CanGoBack)
        {
            frame?.GoBack();
        }
    }
}