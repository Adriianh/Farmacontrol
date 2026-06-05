using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model.UserEntity;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class LoginState : ObservableObject
{
    private readonly UserService _userService;
    private readonly UserSession _userSession;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _rememberMe;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isFirstSetup;

    public string IsFirstSetupTitle => IsFirstSetup
        ? "Configuración Inicial: Crear Administrador Principal"
        : "Inicia sesión para continuar";

    public string IsFirstSetupButton => IsFirstSetup ? "Crear y Entrar" : "Iniciar Sesión";

    public LoginState(UserService userService, UserSession userSession)
    {
        _userService = userService;
        _userSession = userSession;

        CheckIfFirstSetup();
    }

    private void CheckIfFirstSetup()
    {
        IsFirstSetup = !_userService.GetAllUsers().Any();
    }

    [RelayCommand]
    private void Submit()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "El nombre de usuario y contraseña son obligatorios.";
            return;
        }

        if (IsFirstSetup)
        {
            CreateInitialAdmin();
        }
        else
        {
            PerformLogin();
        }
    }

    private void CreateInitialAdmin()
    {
        try
        {
            var admin = new Administrator(Username, Username, Password);
            _userService.AddUser(admin);

            _userSession.SetUser(admin);

            if (RememberMe)
            {
                _userSession.SaveSession();
            }

            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<RootState>(Program.ServiceProvider).NavigateToMain();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear administrador inicial: {ex.Message}";
        }
    }

    private void PerformLogin()
    {
        try
        {
            var user = _userService.Authenticate(Username, Password);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    ErrorMessage = "Este usuario ha sido dado de baja.";
                    return;
                }

                _userSession.SetUser(user);

                if (RememberMe)
                {
                    _userSession.SaveSession();
                }

                Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                    .GetRequiredService<RootState>(Program.ServiceProvider).NavigateToMain();
            }
            else
            {
                ErrorMessage = "Credenciales incorrectas.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
        }
    }
}