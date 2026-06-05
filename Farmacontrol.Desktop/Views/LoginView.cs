using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views;

public class LoginView() : ViewBase<LoginState>(Program.ServiceProvider.GetRequiredService<LoginState>())
{
    private static readonly ISolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#0F172A");
    private static readonly ISolidColorBrush TextMuted = SolidColorBrush.Parse("#94A3B8");
    private static readonly ISolidColorBrush AccentBlue = SolidColorBrush.Parse("#3B82F6");
    private static readonly ISolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");

    private static readonly ISolidColorBrush
        GlassBackground = new ImmutableSolidColorBrush(Color.Parse("#1E293B"), 0.7);

    private static readonly ISolidColorBrush GlassBorder = new ImmutableSolidColorBrush(Color.Parse("#334155"), 0.5);

    protected override object Build(LoginState state)
    {
        var errorTextBlock = new TextBlock()
            .Text(state, s => s.ErrorMessage)
            .Foreground(ErrorRed)
            .TextWrapping(TextWrapping.Wrap)
            .HorizontalAlignment(HorizontalAlignment.Center);

        errorTextBlock.Bind(IsVisibleProperty, new Binding
        {
            Source = state,
            Path = nameof(state.ErrorMessage),
            Converter = new FuncValueConverter<string, bool>(err => !string.IsNullOrEmpty(err))
        });

        return new Grid()
            .Background(BackgroundPrimary)
            .Children(
                new Border().Background(new ImmutableSolidColorBrush(Color.Parse("#3B82F6"), 0.2))
                    .CornerRadius(200).Width(400).Height(400)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .VerticalAlignment(VerticalAlignment.Top)
                    .Margin(-100, -100, 0, 0),
                new Border().Background(new ImmutableSolidColorBrush(Color.Parse("#10B981"), 0.15))
                    .CornerRadius(250).Width(500).Height(500)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .VerticalAlignment(VerticalAlignment.Bottom)
                    .Margin(0, 0, -150, -150),
                new Border()
                    .Background(GlassBackground)
                    .BorderBrush(GlassBorder)
                    .BorderThickness(1)
                    .CornerRadius(16)
                    .Padding(40)
                    .Width(450)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Child(
                        new StackPanel().Spacing(24)
                            .Children(
                                new StackPanel().Spacing(8).HorizontalAlignment(HorizontalAlignment.Center)
                                    .Children(
                                        new TextBlock()
                                            .Text("⚕️ Farmacontrol")
                                            .FontSize(32)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White)
                                            .HorizontalAlignment(HorizontalAlignment.Center),
                                        new TextBlock()
                                            .Text(state, s => s.IsFirstSetupTitle)
                                            .FontSize(14)
                                            .Foreground(TextMuted)
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                            .TextWrapping(TextWrapping.Wrap)
                                            .TextAlignment(TextAlignment.Center)
                                    ),
                                new StackPanel().Spacing(16)
                                    .Children(
                                        new TextBox()
                                            .Text(state, s => s.Username, BindingMode.TwoWay)
                                            .With(t => t.PlaceholderText = "Nombre de Usuario")
                                            .Padding(12)
                                            .CornerRadius(8)
                                            .FontSize(15),
                                        new TextBox()
                                            .Text(state, s => s.Password, BindingMode.TwoWay)
                                            .With(t => t.PlaceholderText = "Contraseña")
                                            .With(t => t.PasswordChar = '•')
                                            .Padding(12)
                                            .CornerRadius(8)
                                            .FontSize(15),
                                        new CheckBox()
                                            .Content("Mantener sesión iniciada")
                                            .IsChecked(state, s => s.RememberMe, BindingMode.TwoWay)
                                            .Foreground(TextMuted)
                                    ),
                                errorTextBlock,
                                new Button()
                                    .Content(state, s => s.IsFirstSetupButton)
                                    .Command(state.SubmitCommand)
                                    .Background(AccentBlue)
                                    .Foreground(Brushes.White)
                                    .FontWeight(FontWeight.Bold)
                                    .FontSize(16)
                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                    .HorizontalContentAlignment(HorizontalAlignment.Center)
                                    .Padding(16)
                                    .CornerRadius(8)
                            )
                    )
            );
    }
}