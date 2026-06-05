using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class RootState : ObservableObject
{
    private readonly UserSession _userSession;

    [ObservableProperty] private object? _currentContent;

    public RootState(UserSession userSession)
    {
        _userSession = userSession;
        InitializeSession();
    }

    private void InitializeSession()
    {
        _userSession.LoadSession();

        if (_userSession.CurrentUser != null)
        {
            CurrentContent = new Views.MainView();
        }
        else
        {
            CurrentContent = new Views.LoginView();
        }
    }

    public void NavigateToMain()
    {
        CurrentContent = new Views.MainView();
    }

    public void NavigateToLogin()
    {
        CurrentContent = new Views.LoginView();
    }
}