using Microsoft.UI.Xaml.Controls;
namespace Boards_WP.Data.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void Initialize(object frame)
    {
        _frame = frame as Frame;
    }

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("NavigationService is not initialized. Call Initialize with a Frame before navigating.");
        }

        _frame.Navigate(pageType, parameter);
    }
    public void GoBack()
    {
        if (CanGoBack)
        {
            _frame?.GoBack();
        }
    }
}