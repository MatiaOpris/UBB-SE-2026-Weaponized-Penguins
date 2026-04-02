using System;
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
        if (_frame != null)
        {
            _frame.Navigate(pageType, parameter);
        }
        else
        {
            _frame.Navigate(pageType);
        }
    }
    public void GoBack()
    {
        if (CanGoBack)
        {
            _frame?.GoBack();
        }
    }
}