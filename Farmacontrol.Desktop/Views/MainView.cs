using System.Linq.Expressions;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Desktop.Views.Administration;
using Farmacontrol.Desktop.Views.Alerts;
using Farmacontrol.Desktop.Views.Inventory;
using Farmacontrol.Desktop.Views.Sales;

namespace Farmacontrol.Desktop.Views;

public partial class MainView() : ViewBase<MainView.State>(new State())
{
    private static readonly SolidColorBrush BackgroundColor = SolidColorBrush.Parse("#F1F5F9");
    private static readonly SolidColorBrush SidebarColor = SolidColorBrush.Parse("#1E293B");
    private static readonly SolidColorBrush SidebarSeparator = SolidColorBrush.Parse("#334155");
    private static readonly SolidColorBrush GroupButtonColor = SolidColorBrush.Parse("#273549");
    private static readonly SolidColorBrush GroupButtonHover = SolidColorBrush.Parse("#334155");
    private static readonly SolidColorBrush GroupTextColor = SolidColorBrush.Parse("#F8FAFC");
    private static readonly SolidColorBrush ArrowColor = SolidColorBrush.Parse("#94A3B8");
    private static readonly SolidColorBrush SubItemColor = SolidColorBrush.Parse("#94A3B8");
    private static readonly SolidColorBrush HeaderTitleColor = SolidColorBrush.Parse("#0F172A");
    private static readonly SolidColorBrush AccentColor = SolidColorBrush.Parse("#3B82F6");

    protected override object Build(State state) =>
        new Grid().Cols("Auto, *")
            .Background(BackgroundColor)
            .Children(
                new Border().RowSpan(2)
                    .Width(state, x => x.SidebarWidth)
                    .Background(SidebarColor)
                    .Padding(12, 15, 12, 10)
                    .Margin(left: 10, top: 10, right: 0, bottom: 10)
                    .CornerRadius(12)
                    .BoxShadow(new BoxShadows(new BoxShadow
                    {
                        Blur = 18, OffsetX = 2, OffsetY = 6,
                        Color = Color.Parse("#33000000")
                    }))
                    .Transitions([
                        new DoubleTransition
                        {
                            Property = WidthProperty,
                            Duration = TimeSpan.FromMilliseconds(250),
                            Easing = new QuadraticEaseOut()
                        }
                    ])
                    .Child(
                        new Grid().Rows("Auto, *, Auto")
                            .Children(
                                new StackPanel()
                                    .Children(
                                        new TextBlock()
                                            .Text("FARMACONTROL")
                                            .FontSize(20)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White)
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                            .Margin(0, 10, 0, 5),
                                        new Separator()
                                            .Background(SidebarSeparator)
                                            .Height(1)
                                            .Margin(horizontal: 8, vertical: 5)
                                    ),
                                new ScrollViewer().Row(1)
                                    .Content(
                                        new StackPanel()
                                            .Margin(0, 10, 0, 0)
                                            .Children(
                                                CreateMenuGroup(state, x => x.SalesExpanded, x => x.SalesArrow,
                                                    "Sales", "📊 Ventas y Caja", [
                                                        CreateSubButton("💵 Registrar Venta",
                                                            () => state.CurrentContent = new RegisterSaleView()),
                                                        CreateSubButton("❌ Anular Venta",
                                                            () => state.CurrentContent = new VoidSaleView()),
                                                        CreateSubButton("📈 Reporte de Ventas",
                                                            () => state.CurrentContent = new SalesReportView())
                                                    ]),
                                                CreateMenuGroup(state, x => x.InventoryExpanded, x => x.InventoryArrow,
                                                    "Inventory", "📦 Gestión de Inventario", [
                                                        CreateSubButton("📋 Ver/Editar Stock",
                                                            () => state.CurrentContent = new InventoryView()),
                                                        CreateSubButton("🔍 Buscar Producto",
                                                            () => state.CurrentContent = new SearchProductView()),
                                                        CreateSubButton("⏳ Medicamentos Vencidos",
                                                            () => state.CurrentContent = new ExpiredProductView()),
                                                        CreateSubButton("📝 Pedidos Pendientes",
                                                            () => state.CurrentContent = new PendingOrdersView())
                                                    ]),
                                                CreateMenuGroup(state, x => x.AlertsExpanded, x => x.AlertsArrow,
                                                    "Alerts", "⚠️ Alertas y Avisos", [
                                                        CreateSubButton("🚨 Alertas Activas",
                                                            () => state.CurrentContent = new AlertsView()),
                                                        CreateSubButton("📜 Historial de Alertas",
                                                            () => state.CurrentContent = new AlertsHistoryView())
                                                    ]),
                                                CreateMenuGroup(state, x => x.AdminExpanded, x => x.AdminArrow,
                                                    "Admin", "⚙️ Administración", [
                                                        CreateSubButton("👥 Gestionar Usuarios",
                                                            () => state.CurrentContent = new UsersView()),
                                                        CreateSubButton("🚚 Proveedores",
                                                            () => state.CurrentContent = new SuppliersView()),
                                                    ])
                                            )
                                    )
                            )
                    ),
                new Grid().Col(1).Rows("70, *")
                    .Children(
                        new Border()
                            .Background(Brushes.Transparent)
                            .Padding(horizontal: 30, vertical: 0)
                            .Child(
                                new Grid().Cols("Auto, *, Auto")
                                    .Children(
                                        new Button()
                                            .Content("☰")
                                            .Background(Brushes.Transparent)
                                            .Foreground(SidebarColor)
                                            .FontSize(18)
                                            .Margin(0, 0, 15, 0)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .OnClick(_ => state.SidebarExpanded = !state.SidebarExpanded),
                                        new TextBlock().Col(1)
                                            .Text(state, x => x.ScreenTitle)
                                            .FontSize(20)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(HeaderTitleColor)
                                            .VerticalAlignment(VerticalAlignment.Center),
                                        new TextBlock().Col(2)
                                            .Text("🟢 DB: SQLite Conectado")
                                            .FontSize(12)
                                            .Foreground(Brushes.Green)
                                            .FontWeight(FontWeight.Medium)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                    )
                            ),
                        new Border().Row(1)
                            .Margin(left: 30, top: 0, right: 30, bottom: 20)
                            .Background(Brushes.White)
                            .CornerRadius(12)
                            .BoxShadow(new BoxShadows(new BoxShadow
                            {
                                Blur = 12, OffsetX = 0, OffsetY = 4,
                                Color = Color.Parse("#1A000000")
                            }))
                            .Child(
                                new ContentControl()
                                    .Content<ContentControl, State, object>(state, x => x.CurrentContent)
                            )
                    )
            );

    private Control CreateMenuGroup(
        State state,
        Expression<Func<State, bool>> bindingGetter,
        Expression<Func<State, string>> arrowGetter,
        string identifier,
        string title,
        Control[] subItems)
    {
        var headerButton = new Button()
            .Background(GroupButtonColor)
            .Foreground(GroupTextColor)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .CornerRadius(8)
            .Margin(0, 0, 0, 4)
            .Padding(horizontal: 16, vertical: 12)
            .Content(
                new Grid().Cols("*, Auto")
                    .Children(
                        new TextBlock()
                            .Text(title)
                            .FontWeight(FontWeight.SemiBold)
                            .VerticalAlignment(VerticalAlignment.Center),
                        new TextBlock().Col(1)
                            .Text(state, arrowGetter)
                            .Foreground(ArrowColor)
                            .FontSize(11)
                            .VerticalAlignment(VerticalAlignment.Center)
                    )
            );

        headerButton.Styles.Add(
            new Style(x => x.OfType<Button>().Class(":pointerover").Template().OfType<ContentPresenter>())
            {
                Setters =
                {
                    new Setter(ContentPresenter.ForegroundProperty, GroupTextColor),
                    new Setter(ContentPresenter.BackgroundProperty, GroupButtonHover)
                }
            });

        var animatedContainer = new Border()
            .ClipToBounds(true)
            .Background(Brushes.Transparent)
            .Transitions([
                new DoubleTransition
                {
                    Property = MaxHeightProperty,
                    Duration = TimeSpan.FromMilliseconds(250),
                    Easing = new QuadraticEaseOut()
                }
            ])
            .Child(
                new StackPanel()
                    .Margin(left: 12, top: 4, right: 0, bottom: 8)
                    .Children(subItems)
            );

        var isExpanded = bindingGetter.Compile();

        headerButton.Click += (_, _) =>
        {
            state.ExpandedCategory = state.ExpandedCategory == identifier ? "" : identifier;
        };

        animatedContainer.MaxHeight = isExpanded(state) ? 300 : 0;

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.ExpandedCategory))
                animatedContainer.MaxHeight = isExpanded(state) ? 300 : 0;
        };

        return new StackPanel()
            .Margin(0, 0, 0, 4)
            .Children(headerButton, animatedContainer);
    }

    private Button CreateSubButton(string text, Action action)
    {
        var button = new Button()
            .Content(text)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Left)
            .Background(Brushes.Transparent)
            .Foreground(SubItemColor)
            .CornerRadius(6)
            .Margin(0, 0, 0, 4)
            .Padding(horizontal: 12, vertical: 8);

        button.Styles.Add(
            new Style(x => x.OfType<Button>().Class(":pointerover").Template().OfType<ContentPresenter>())
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, AccentColor),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White)
                }
            });

        button.Click += (_, _) => action();

        return button;
    }

    public partial class State : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SidebarWidth))]
        public partial bool SidebarExpanded { get; set; } = true;

        public double SidebarWidth => SidebarExpanded ? 240 : 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SalesExpanded)),    NotifyPropertyChangedFor(nameof(SalesArrow))]
        [NotifyPropertyChangedFor(nameof(InventoryExpanded)),NotifyPropertyChangedFor(nameof(InventoryArrow))]
        [NotifyPropertyChangedFor(nameof(AlertsExpanded)),   NotifyPropertyChangedFor(nameof(AlertsArrow))]
        [NotifyPropertyChangedFor(nameof(AdminExpanded)),    NotifyPropertyChangedFor(nameof(AdminArrow))]
        public partial string ExpandedCategory { get; set; } = "Inventory";

        public bool SalesExpanded => ExpandedCategory == "Sales";
        public bool InventoryExpanded => ExpandedCategory == "Inventory";
        public bool AlertsExpanded => ExpandedCategory == "Alerts";
        public bool AdminExpanded => ExpandedCategory == "Admin";

        public string SalesArrow => SalesExpanded ? "▲" : "▼";
        public string InventoryArrow => InventoryExpanded ? "▲" : "▼";
        public string AlertsArrow => AlertsExpanded ? "▲" : "▼";
        public string AdminArrow => AdminExpanded ? "▲" : "▼";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ScreenTitle))]
        public partial object CurrentContent { get; set; } = new InventoryView();

        public string ScreenTitle => CurrentContent switch
        {
            InventoryView => "Administración de Inventario",
            AlertsView => "Alertas de Inventario",
            _ => "Panel de Control"
        };
    }
}