using Farmacontrol.Services;

namespace Farmacontrol.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly UserSession _userSession;
        private readonly IServiceProvider _serviceProvider;

        private Label lblTitle = null!;
        private Label lblUserInfo = null!;
        private Button btnSales = null!;
        private Button btnInventory = null!;
        private Button btnSearchProduct = null!;
        private Button btnAlerts = null!;
        private Button btnAlertHistory = null!;
        private Button btnReports = null!;
        private Button btnExpiredProducts = null!;
        private Button btnUsers = null!;
        private Button btnSuppliers = null!;
        private Button btnOrders = null!;
        private Button btnVoidSale = null!;
        private Button btnLogout = null!;
        private Button btnExit = null!;

        public MainForm(
            UserSession userSession,
            IServiceProvider serviceProvider)
        {
            _userSession = userSession;
            _serviceProvider = serviceProvider;

            InitializeComponent();
            ConfigureUserInfo();
            ConfigurePermissions();
        }

        private void InitializeComponent()
        {
            Text = "Farmacontrol - Menú principal";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(780, 520);
            MinimumSize = new Size(780, 520);
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "FARMACONTROL",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 78, 112),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 20),
                Size = new Size(780, 45)
            };

            lblUserInfo = new Label
            {
                Text = "Usuario:",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.DimGray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 70),
                Size = new Size(780, 25)
            };

            btnSales = CreateMenuButton("Registrar venta", 70, 125);
            btnInventory = CreateMenuButton("Gestionar inventario", 290, 125);
            btnSearchProduct = CreateMenuButton("Buscar producto", 510, 125);

            btnAlerts = CreateMenuButton("Ver alertas", 70, 185);
            btnAlertHistory = CreateMenuButton("Historial de alertas", 290, 185);
            btnReports = CreateMenuButton("Reportes de ventas", 510, 185);

            btnExpiredProducts = CreateMenuButton("Productos vencidos", 70, 245);
            btnUsers = CreateMenuButton("Gestionar usuarios", 290, 245);
            btnSuppliers = CreateMenuButton("Gestionar proveedores", 510, 245);

            btnOrders = CreateMenuButton("Generar pedidos", 70, 305);
            btnVoidSale = CreateMenuButton("Anular venta", 290, 305);

            btnLogout = new Button
            {
                Text = "Cerrar sesión",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(470, 430),
                Size = new Size(120, 35)
            };
            btnLogout.Click += BtnLogout_Click;

            btnExit = new Button
            {
                Text = "Salir",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(610, 430),
                Size = new Size(100, 35)
            };
            btnExit.Click += (_, _) => Application.Exit();

            btnSales.Click += (_, _) => ShowModulePendingMessage("Registro de ventas");
            btnInventory.Click += (_, _) => ShowModulePendingMessage("Gestión de inventario");
            btnSearchProduct.Click += (_, _) => ShowModulePendingMessage("Búsqueda de productos");
            btnAlerts.Click += (_, _) => ShowModulePendingMessage("Alertas");
            btnAlertHistory.Click += (_, _) => ShowModulePendingMessage("Historial de alertas");
            btnReports.Click += (_, _) => ShowModulePendingMessage("Reportes");
            btnExpiredProducts.Click += (_, _) => ShowModulePendingMessage("Productos vencidos");
            btnUsers.Click += (_, _) => ShowModulePendingMessage("Gestión de usuarios");
            btnSuppliers.Click += (_, _) => ShowModulePendingMessage("Gestión de proveedores");
            btnOrders.Click += (_, _) => ShowModulePendingMessage("Generación de pedidos");
            btnVoidSale.Click += (_, _) => ShowModulePendingMessage("Anulación de ventas");

            Controls.Add(lblTitle);
            Controls.Add(lblUserInfo);

            Controls.Add(btnSales);
            Controls.Add(btnInventory);
            Controls.Add(btnSearchProduct);
            Controls.Add(btnAlerts);
            Controls.Add(btnAlertHistory);
            Controls.Add(btnReports);
            Controls.Add(btnExpiredProducts);
            Controls.Add(btnUsers);
            Controls.Add(btnSuppliers);
            Controls.Add(btnOrders);
            Controls.Add(btnVoidSale);
            Controls.Add(btnLogout);
            Controls.Add(btnExit);
        }

        private Button CreateMenuButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(x, y),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(240, 245, 248),
                FlatStyle = FlatStyle.Flat
            };
        }

        private void ConfigureUserInfo()
        {
            var user = _userSession.CurrentUser;

            if (user == null)
            {
                MessageBox.Show(
                    "No hay una sesión activa.",
                    "Sesión inválida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            lblUserInfo.Text = $"Usuario: {user.Name} | Rol: {user.Role}";
        }

        private void ConfigurePermissions()
        {
            var user = _userSession.CurrentUser;

            if (user == null)
                return;

            btnSales.Enabled = HasPermission("1");
            btnInventory.Enabled = HasPermission("2");
            btnSearchProduct.Enabled = HasPermission("3");
            btnAlerts.Enabled = HasPermission("4");
            btnAlertHistory.Enabled = HasPermission("5");
            btnReports.Enabled = HasPermission("6");
            btnExpiredProducts.Enabled = HasPermission("7");
            btnUsers.Enabled = HasPermission("8");
            btnSuppliers.Enabled = HasPermission("9");
            btnOrders.Enabled = HasPermission("10");
            btnVoidSale.Enabled = HasPermission("11");
        }

        private bool HasPermission(string option)
        {
            var user = _userSession.CurrentUser;
            return user?.GetAllowedActions().Any(action => action.StartsWith(option + ".")) == true;
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            _userSession.CurrentUser = null;

            var loginForm = (LoginForm)_serviceProvider.GetService(typeof(LoginForm))!;
            loginForm.Show();

            Close();
        }

        private void ShowModulePendingMessage(string moduleName)
        {
            MessageBox.Show(
                $"El módulo '{moduleName}' será integrado en la siguiente fase.",
                "Módulo en construcción",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}