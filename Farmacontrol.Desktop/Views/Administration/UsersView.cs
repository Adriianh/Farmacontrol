using Avalonia.Controls.Templates;
using Avalonia.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.UserEntity;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Administration;

public class UsersView() : ViewBase<UsersState>(Program.ServiceProvider.GetRequiredService<UsersState>())
{
    private static readonly ISolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#0F172A");
    private static readonly ISolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1E293B");
    private static readonly ISolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#334155");
    private static readonly ISolidColorBrush TextMuted = SolidColorBrush.Parse("#94A3B8");
    private static readonly ISolidColorBrush AccentBlue = SolidColorBrush.Parse("#3B82F6");
    private static readonly ISolidColorBrush SuccessGreen = SolidColorBrush.Parse("#10B981");
    private static readonly ISolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");

    protected override object Build(UsersState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid()
                    .Rows("Auto, *")
                    .Children(
                        BuildHeader().Row(0),
                        BuildContent(state).Row(1)
                    )
            );
    }

    private Control BuildHeader()
    {
        return new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Gestión de Usuarios")
                            .FontSize(26)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Administración de personal y permisos del sistema")
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    )
            ).Margin(0, 0, 0, 24);
    }

    private Control BuildContent(UsersState state)
    {
        return new Grid().Cols("*, 300").ColumnSpacing(24)
            .Children(
                BuildUsersList(state).Col(0),
                BuildCreateUserPanel(state).Col(1)
            );
    }

    private Control BuildUsersList(UsersState state)
    {
        var list = new ListBox()
            .ItemsSource(state.UsersList)
            .ItemTemplate(new FuncDataTemplate<User>((user, _) =>
            {
                var deleteButton = new Button()
                    .Content("Eliminar")
                    .Background(SolidColorBrush.Parse("#451A1A"))
                    .Foreground(ErrorRed)
                    .CornerRadius(6)
                    .Padding(8, 4)
                    .Cursor(new Cursor(StandardCursorType.Hand))
                    .Command(state.DeleteUserCommand)
                    .CommandParameter(user.Username);

                deleteButton.Bind(IsVisibleProperty, new Binding
                {
                    Source = state,
                    Path = nameof(state.IsAdmin)
                });

                var isAdministrator = user is Administrator;

                return new Border()
                    .Background(BackgroundSecondary)
                    .CornerRadius(8)
                    .Padding(16)
                    .Margin(0, 0, 0, 8)
                    .Child(
                        new Grid().Cols("Auto, *, Auto, Auto")
                            .Children(
                                new Border()
                                    .Background(BackgroundTertiary)
                                    .CornerRadius(20)
                                    .Width(40).Height(40)
                                    .Col(0).Margin(0, 0, 16, 0)
                                    .Child(
                                        new TextBlock().Text(user.Username.Substring(0, 1).ToUpper())
                                            .Foreground(Brushes.White)
                                            .FontWeight(FontWeight.Bold)
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                    ),
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(1)
                                    .Children(
                                        new TextBlock().Text(user.Username).FontWeight(FontWeight.SemiBold)
                                            .Foreground(Brushes.White),
                                        new TextBlock().Text(isAdministrator ? "Administrador" : "Empleado")
                                            .FontSize(13).Foreground(TextMuted)
                                    ),
                                new Border().Background(SolidColorBrush.Parse("#1A4526")).CornerRadius(16).Padding(8, 4)
                                    .Col(2).VerticalAlignment(VerticalAlignment.Center).Margin(0, 0, 16, 0)
                                    .Child(new TextBlock().Text("Activo").Foreground(SuccessGreen).FontSize(12)
                                        .FontWeight(FontWeight.Bold)),
                                deleteButton.Col(3).VerticalAlignment(VerticalAlignment.Center)
                            )
                    );
            }))
            .Background(Brushes.Transparent)
            .BorderBrush(Brushes.Transparent);

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid().Rows("Auto, *").Children(
                    new TextBlock().Text("Lista de Usuarios").FontSize(18).FontWeight(FontWeight.Bold)
                        .Foreground(Brushes.White).Row(0).Margin(0, 0, 0, 16),
                    list.Row(1)
                )
            );
    }

    private Control BuildCreateUserPanel(UsersState state)
    {
        var panel = new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new StackPanel().Spacing(16)
                    .Children(
                        new TextBlock().Text("Crear Nuevo Usuario").FontSize(18).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock().Text("Si eres administrador, puedes agregar nuevos usuarios al sistema.")
                            .Foreground(TextMuted).TextWrapping(TextWrapping.Wrap).FontSize(13),
                        new TextBox()
                            .With(c => c.PlaceholderText = "Nombre de Usuario")
                            .Text(state, s => s.NewUsername, BindingMode.TwoWay)
                            .IsEnabled(state, s => s.IsAdmin),
                        new TextBox()
                            .With(c => c.PlaceholderText = "Contraseña")
                            .With(c => c.PasswordChar = '•')
                            .Text(state, s => s.NewPassword, BindingMode.TwoWay)
                            .IsEnabled(state, s => s.IsAdmin),
                        new ComboBox()
                            .With(c => c.PlaceholderText = "Rol del Usuario")
                            .ItemsSource(new[] { "Empleado", "Administrador" })
                            .SelectedIndex(state, s => s.SelectedRoleIndex, BindingMode.TwoWay)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .IsEnabled(state, s => s.IsAdmin),
                        new Button()
                            .Content("Crear Usuario")
                            .Background(AccentBlue)
                            .Foreground(Brushes.White)
                            .FontWeight(FontWeight.Bold)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                            .CornerRadius(6)
                            .Padding(8, 10)
                            .Command(state.CreateUserCommand)
                            .IsEnabled(state, s => s.IsAdmin),
                        new TextBlock()
                            .Text(state, s => s.ErrorMessage)
                            .Foreground(ErrorRed)
                            .TextWrapping(TextWrapping.Wrap),
                        new TextBlock()
                            .Text(state, s => s.SuccessMessage)
                            .Foreground(SuccessGreen)
                            .TextWrapping(TextWrapping.Wrap)
                    )
            );

        return panel;
    }
}