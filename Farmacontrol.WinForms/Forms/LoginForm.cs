using Farmacontrol.Model;
using Farmacontrol.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.WinForms.Forms
{
    public class LoginForm : Form
    {
        private readonly UserManager _userManager;
        private readonly UserSession _userSession;
        private readonly IServiceProvider _serviceProvider;

        private Label lblTitle = null!;
        private Label lblUsername = null!;
        private Label lblPassword = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnExit = null!;

        public LoginForm(
            UserManager userManager,
            UserSession userSession,
            IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _userSession = userSession;
            _serviceProvider = serviceProvider;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Farmacontrol - Inicio de sesión";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 300);
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "FARMACONTROL",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 78, 112),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 25),
                Size = new Size(420, 45)
            };

            lblUsername = new Label
            {
                Text = "Usuario:",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(70, 95),
                Size = new Size(100, 25)
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(170, 92),
                Size = new Size(180, 25)
            };

            lblPassword = new Label
            {
                Text = "Contraseña:",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(70, 135),
                Size = new Size(100, 25)
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(170, 132),
                Size = new Size(180, 25),
                PasswordChar = '*'
            };

            btnLogin = new Button
            {
                Text = "Ingresar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(35, 78, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(170, 185),
                Size = new Size(85, 35)
            };
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button
            {
                Text = "Salir",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(265, 185),
                Size = new Size(85, 35)
            };
            btnExit.Click += (_, _) => Close();

            Controls.Add(lblTitle);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnExit);

            AcceptButton = btnLogin;
            CancelButton = btnExit;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Ingrese su nombre de usuario.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Ingrese su contraseña.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Focus();
                return;
            }

            User? user = _userManager.Authenticate(username, password);

            if (user == null)
            {
                MessageBox.Show(
                    "Usuario o contraseña incorrectos.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            _userSession.CurrentUser = user;

            var mainForm = _serviceProvider.GetRequiredService<MainForm>();
            mainForm.FormClosed += (_, _) => Close();
            mainForm.Show();

            Hide();
        }
    }
}