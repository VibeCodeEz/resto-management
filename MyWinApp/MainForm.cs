using RestaurantOrderManagement.Services;
using RestaurantOrderManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderManagement
{
    public partial class MainForm : Form
    {
        private OrderService _orderService = null!;
        private MenuService _menuService = null!;
        private PaymentService _paymentService = null!;
        private RestaurantDbContext _dbContext = null!;

        public MainForm()
        {
            InitializeComponent();
            InitializeDatabase();
            _orderService = new OrderService(_dbContext);
            _menuService = new MenuService(_dbContext);
            _paymentService = new PaymentService(_dbContext);
            InitializeUI();
        }

        private void InitializeDatabase()
        {
            _dbContext = new RestaurantDbContext();
            _dbContext.Database.EnsureCreated();
        }

        private void InitializeUI()
        {
            this.Text = "Restaurant Order Management System";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Create title label
            var titleLabel = new Label
            {
                Text = "Restaurant Order Management System",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            // Create subtitle
            var subtitleLabel = new Label
            {
                Text = "Select a module to get started",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Create button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(50)
            };

            // Create buttons
            var dineInButton = CreateModuleButton("Dine-In Orders", "Manage dine-in orders and table assignments", Color.FromArgb(52, 152, 219));
            var takeoutButton = CreateModuleButton("Takeout Orders", "Handle takeout orders and customer pickup", Color.FromArgb(46, 204, 113));
            var kitchenButton = CreateModuleButton("Kitchen Dashboard", "Kitchen order management and preparation", Color.FromArgb(231, 76, 60));
            var menuButton = CreateModuleButton("Menu Management", "Add, edit, and manage menu items", Color.FromArgb(155, 89, 182));
            var paymentButton = CreateModuleButton("Payment Processing", "Process payments and print receipts", Color.FromArgb(230, 126, 34));
            var reportsButton = CreateModuleButton("Reports & Analytics", "View sales reports and analytics", Color.FromArgb(241, 196, 15));

            // Add click events
            dineInButton.Click += (s, e) => OpenDineInForm();
            takeoutButton.Click += (s, e) => OpenTakeoutForm();
            kitchenButton.Click += (s, e) => OpenKitchenForm();
            menuButton.Click += (s, e) => OpenMenuManagementForm();
            paymentButton.Click += (s, e) => OpenPaymentForm();
            reportsButton.Click += (s, e) => OpenReportsForm();

            // Layout buttons in a grid
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(20)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            tableLayout.Controls.Add(dineInButton, 0, 0);
            tableLayout.Controls.Add(takeoutButton, 1, 0);
            tableLayout.Controls.Add(kitchenButton, 2, 0);
            tableLayout.Controls.Add(menuButton, 0, 1);
            tableLayout.Controls.Add(paymentButton, 1, 1);
            tableLayout.Controls.Add(reportsButton, 2, 1);

            buttonPanel.Controls.Add(tableLayout);

            // Add controls to form
            this.Controls.Add(buttonPanel);
            this.Controls.Add(subtitleLabel);
            this.Controls.Add(titleLabel);
        }

        private Button CreateModuleButton(string title, string description, Color color)
        {
            var button = new Button
            {
                Text = $"{title}\n\n{description}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color);

            return button;
        }

        private void OpenDineInForm()
        {
            var dineInForm = new DineInForm(_orderService, _menuService);
            dineInForm.Show();
        }

        private void OpenTakeoutForm()
        {
            var takeoutForm = new TakeoutForm(_orderService, _menuService);
            takeoutForm.Show();
        }

        private void OpenKitchenForm()
        {
            var kitchenForm = new KitchenForm(_orderService);
            kitchenForm.Show();
        }

        private void OpenMenuManagementForm()
        {
            var menuForm = new MenuManagementForm(_menuService);
            menuForm.Show();
        }

        private void OpenPaymentForm()
        {
            var paymentForm = new PaymentForm(_paymentService, _orderService);
            paymentForm.Show();
        }

        private void OpenReportsForm()
        {
            var reportsForm = new ReportsForm(_orderService, _paymentService);
            reportsForm.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnFormClosing(e);
        }
    }
} 