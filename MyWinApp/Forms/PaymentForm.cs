using RestaurantOrderManagement.Models;
using RestaurantOrderManagement.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RestaurantOrderManagement
{
    public partial class PaymentForm : Form
    {
        private readonly PaymentService _paymentService;
        private readonly OrderService _orderService;
        private Order? _selectedOrder;
        private ListView _ordersListView;
        private ListView _orderItemsListView;
        private Label _totalLabel;
        private TextBox _amountPaidTextBox;
        private Label _changeLabel;
        private TextBox _cashierNameTextBox;
        private TextBox _notesTextBox;
        private Button _processPaymentButton;
        private Button _printReceiptButton;

        public PaymentForm(PaymentService paymentService, OrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            InitializeComponent();
            InitializeUI();
            LoadUnpaidOrders();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Name = "PaymentForm";
            this.Text = "Payment Processing";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            // Elegant dark background
            this.BackColor = Color.FromArgb(28, 28, 34); // Deep charcoal

            // Logo placeholder
            var logoPanel = new Panel
            {
                Height = 70,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(22, 22, 28)
            };
            var logoLabel = new Label
            {
                Text = "[Your Logo]",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.Gold,
                Dock = DockStyle.Left,
                Width = 220,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            logoPanel.Controls.Add(logoLabel);

            // Title
            var titleLabel = new Label
            {
                Text = "Payment Processing",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gold,
                BackColor = Color.FromArgb(28, 28, 34)
            };

            // Main layout
            var mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 400,
                BackColor = Color.FromArgb(28, 28, 34)
            };

            // Left panel - Orders and payment form
            var leftPanel = CreateLeftPanel();
            mainSplitContainer.Panel1.Controls.Add(leftPanel);

            // Right panel - Order details
            var rightPanel = CreateRightPanel();
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(titleLabel);
            this.Controls.Add(logoPanel);
        }

        private Panel CreateLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(34, 34, 40) };

            // Orders list
            var ordersGroup = new GroupBox
            {
                Text = "Unpaid Orders",
                Dock = DockStyle.Top,
                Height = 300,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(34, 34, 40),
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            ordersGroup.FlatStyle = FlatStyle.Flat;

            _ordersListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(44, 44, 54),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None
            };
            _ordersListView.Columns.Add("Order ID", 80);
            _ordersListView.Columns.Add("Customer", 120);
            _ordersListView.Columns.Add("Type", 80);
            _ordersListView.Columns.Add("Total", 100);
            _ordersListView.SelectedIndexChanged += OrdersListView_SelectedIndexChanged;

            ordersGroup.Controls.Add(_ordersListView);

            // Payment form
            var paymentGroup = new GroupBox
            {
                Text = "Payment Details",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(34, 34, 40),
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            paymentGroup.FlatStyle = FlatStyle.Flat;

            var formPanel = new Panel { Dock = DockStyle.Fill };

            // Total amount
            var totalLabel = new Label { Text = "Total Amount:", Location = new Point(10, 20), Width = 120, ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            _totalLabel = new Label { Location = new Point(140, 20), Width = 150, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Gold, BackColor = Color.FromArgb(44, 44, 54), BorderStyle = BorderStyle.FixedSingle, Height = 30, TextAlign = ContentAlignment.MiddleLeft };

            // Amount paid
            var amountPaidLabel = new Label { Text = "Amount Paid:", Location = new Point(10, 60), Width = 120, ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            _amountPaidTextBox = new TextBox { Location = new Point(140, 57), Width = 150, Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(44, 44, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle }; 
            _amountPaidTextBox.TextChanged += (s, e) => ValidatePaymentForm();

            // Change
            var changeLabel = new Label { Text = "Change:", Location = new Point(10, 100), Width = 120, ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            _changeLabel = new Label { Location = new Point(140, 100), Width = 150, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Gold, BackColor = Color.FromArgb(44, 44, 54), BorderStyle = BorderStyle.FixedSingle, Height = 30, TextAlign = ContentAlignment.MiddleLeft };

            // Cashier name
            var cashierLabel = new Label { Text = "Cashier:", Location = new Point(10, 140), Width = 120, ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            _cashierNameTextBox = new TextBox { Location = new Point(140, 137), Width = 150, Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(44, 44, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            _cashierNameTextBox.TextChanged += (s, e) => ValidatePaymentForm();

            // Notes
            var notesLabel = new Label { Text = "Notes:", Location = new Point(10, 180), Width = 120, ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            _notesTextBox = new TextBox { Location = new Point(140, 177), Width = 200, Height = 60, Multiline = true, Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(44, 44, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // Buttons
            var buttonPanel = new Panel { Location = new Point(10, 260), Width = 350, Height = 50, BackColor = Color.Transparent };

            _processPaymentButton = new Button
            {
                Text = "Process Payment",
                Location = new Point(0, 0),
                Width = 140,
                Height = 40,
                BackColor = Color.Gold,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Enabled = false
            };
            _processPaymentButton.FlatAppearance.BorderSize = 0;
            _processPaymentButton.Region = System.Drawing.Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, _processPaymentButton.Width, _processPaymentButton.Height, 18, 18));
            _processPaymentButton.Click += ProcessPaymentButton_Click;

            _printReceiptButton = new Button
            {
                Text = "Print Receipt",
                Location = new Point(160, 0),
                Width = 140,
                Height = 40,
                BackColor = Color.FromArgb(44, 44, 54),
                ForeColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Enabled = false
            };
            _printReceiptButton.FlatAppearance.BorderSize = 0;
            _printReceiptButton.Region = System.Drawing.Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, _printReceiptButton.Width, _printReceiptButton.Height, 18, 18));
            _printReceiptButton.Click += PrintReceiptButton_Click;

            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(310, 0),
                Width = 80,
                Height = 40,
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Region = System.Drawing.Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, refreshButton.Width, refreshButton.Height, 18, 18));
            refreshButton.Click += RefreshButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { _processPaymentButton, _printReceiptButton, refreshButton });

            formPanel.Controls.AddRange(new Control[] {
                totalLabel, _totalLabel,
                amountPaidLabel, _amountPaidTextBox,
                changeLabel, _changeLabel,
                cashierLabel, _cashierNameTextBox,
                notesLabel, _notesTextBox,
                buttonPanel
            });

            paymentGroup.Controls.Add(formPanel);

            panel.Controls.Add(paymentGroup);
            panel.Controls.Add(ordersGroup);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var titleLabel = new Label
            {
                Text = "Order Details",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _orderItemsListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };
            _orderItemsListView.Columns.Add("Item", 200);
            _orderItemsListView.Columns.Add("Qty", 50);
            _orderItemsListView.Columns.Add("Price", 80);
            _orderItemsListView.Columns.Add("Total", 80);

            panel.Controls.Add(_orderItemsListView);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void LoadUnpaidOrders()
        {
            _ordersListView.Items.Clear();
            var unpaidOrders = _orderService.GetAllOrders()
                .Where(o => o.Status == OrderStatus.Completed && !o.IsPaid)
                .OrderByDescending(o => o.OrderTime)
                .ToList();

            foreach (var order in unpaidOrders)
            {
                var item = new ListViewItem(order.OrderId.ToString());
                item.SubItems.Add(order.CustomerName);
                item.SubItems.Add(order.Type.ToString());
                item.SubItems.Add($"${order.Items.Sum(i => i.Price * i.Quantity):F2}");
                item.Tag = order;
                _ordersListView.Items.Add(item);
            }
        }

        private void OrdersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ordersListView.SelectedItems.Count > 0)
            {
                _selectedOrder = (Order)_ordersListView.SelectedItems[0].Tag;
                LoadOrderDetails();
                UpdatePaymentForm();
            }
            else
            {
                _selectedOrder = null;
                ClearPaymentForm();
            }
        }

        private void LoadOrderDetails()
        {
            _orderItemsListView.Items.Clear();
            if (_selectedOrder != null)
            {
                foreach (var item in _selectedOrder.Items)
                {
                    var listItem = new ListViewItem(item.Name);
                    listItem.SubItems.Add(item.Quantity.ToString());
                    listItem.SubItems.Add($"${item.Price:F2}");
                    listItem.SubItems.Add($"${item.Price * item.Quantity:F2}");
                    _orderItemsListView.Items.Add(listItem);
                }
            }
        }

        private void UpdatePaymentForm()
        {
            if (_selectedOrder != null)
            {
                var total = _selectedOrder.Items.Sum(item => item.Price * item.Quantity);
                _totalLabel.Text = $"${total:F2}";
                _amountPaidTextBox.Text = total.ToString("F2");
                _processPaymentButton.Enabled = true;
            }
        }

        private void ClearPaymentForm()
        {
            _totalLabel.Text = "$0.00";
            _amountPaidTextBox.Clear();
            _changeLabel.Text = "$0.00";
            _cashierNameTextBox.Clear();
            _notesTextBox.Clear();
            _processPaymentButton.Enabled = false;
            _printReceiptButton.Enabled = false;
        }

        private void ValidatePaymentForm()
        {
            if (_selectedOrder != null && decimal.TryParse(_amountPaidTextBox.Text, out var amountPaid))
            {
                var total = _selectedOrder.Items.Sum(item => item.Price * item.Quantity);
                var change = amountPaid - total;
                _changeLabel.Text = $"${change:F2}";
                bool valid = amountPaid >= total && !string.IsNullOrWhiteSpace(_cashierNameTextBox.Text);
                _processPaymentButton.Enabled = valid;
            }
            else
            {
                _changeLabel.Text = "$0.00";
                _processPaymentButton.Enabled = false;
            }
            _printReceiptButton.Enabled = false;
        }

        private void ProcessPaymentButton_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;
            try
            {
                var amountPaid = decimal.Parse(_amountPaidTextBox.Text);
                var cashierName = _cashierNameTextBox.Text.Trim();
                var notes = _notesTextBox.Text.Trim();
                var payment = _paymentService.ProcessCashPayment(
                    _selectedOrder.OrderId,
                    amountPaid,
                    cashierName,
                    notes
                );
                MessageBox.Show($"Payment processed successfully!\nReceipt #: {payment.ReceiptNumber}\nChange: ${payment.Change:F2}",
                    "Payment Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _printReceiptButton.Enabled = true;
                LoadUnpaidOrders();
                ClearPaymentForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.InnerException?.Message ?? ex.Message}", "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReceiptButton_Click(object sender, EventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("No order selected for receipt printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var payment = _paymentService.GetPaymentByOrderId(_selectedOrder.OrderId);
            if (payment != null)
            {
                _paymentService.PrintReceipt(payment);
            }
            else
            {
                MessageBox.Show("No payment found for this order. Please process payment first.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadUnpaidOrders();
            ClearPaymentForm();
        }

        // Add this helper class for rounded corners
        internal static class NativeMethods
        {
            [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }
    }
} 