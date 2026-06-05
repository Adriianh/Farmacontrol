using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.UserEntity;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class UsersState : ObservableObject
{
    private readonly UserService _userService;
    private readonly UserSession _userSession;

    public ObservableCollection<User> UsersList { get; } = new();

    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _newUsername = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private int _selectedRoleIndex;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public UsersState(UserService userService, UserSession userSession)
    {
        _userService = userService;
        _userSession = userSession;

        IsAdmin = _userSession.CurrentUser is Administrator;

        LoadUsers();
    }

    [RelayCommand]
    private void LoadUsers()
    {
        UsersList.Clear();
        var allUsers = _userService.GetAllUsers().Where(u => u.IsActive);
        foreach (var user in allUsers)
        {
            UsersList.Add(user);
        }
    }

    [RelayCommand]
    private void CreateUser()
    {
        if (!IsAdmin)
        {
            ErrorMessage = "No tienes permisos para crear usuarios.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "El nombre de usuario y la contraseña son requeridos.";
            return;
        }

        if (UsersList.Any(u => u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase)))
        {
            ErrorMessage = "El nombre de usuario ya existe.";
            return;
        }

        try
        {
            User newUser;
            if (SelectedRoleIndex == 1)
            {
                newUser = new Administrator(NewUsername, NewUsername, NewPassword);
            }
            else
            {
                newUser = new Employee(NewUsername, NewUsername, NewPassword);
            }

            _userService.AddUser(newUser);

            NewUsername = string.Empty;
            NewPassword = string.Empty;
            ErrorMessage = string.Empty;
            SuccessMessage = $"Usuario '{newUser.Username}' creado exitosamente.";

            LoadUsers();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear usuario: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DeleteUser(string username)
    {
        if (!IsAdmin) return;

        if (username == _userSession.CurrentUser?.Username)
        {
            ErrorMessage = "No puedes eliminar tu propio usuario.";
            return;
        }

        try
        {
            _userService.RemoveUser(username);
            SuccessMessage = $"Usuario '{username}' eliminado correctamente.";
            LoadUsers();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al eliminar: {ex.Message}";
        }
    }
}