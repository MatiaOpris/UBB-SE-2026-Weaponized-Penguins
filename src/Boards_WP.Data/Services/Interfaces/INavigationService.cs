using System;
using System.Collections.Generic;
using System.Text;

//using Microsoft.UI.Xaml.Controls;
namespace Boards_WP.Data.Services.Interfaces;



public interface INavigationService
{

    void Initialize(object frame);

    void NavigateTo(Type pageType, object? parameter = null);
    void GoBack();
    bool CanGoBack { get; }
}