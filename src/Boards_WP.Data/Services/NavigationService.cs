using System;
using System.Collections.Generic;
using System.Text;

namespace Boards_WP.Data.Services;

public class NavigationService : INavigationService
{
    public bool CanGoBack => throw new NotImplementedException();

    public void GoBack()
    {
        throw new NotImplementedException();
    }

    public void Initialize(object frame)
    {
        throw new NotImplementedException();
    }

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        throw new NotImplementedException();
    }
}
