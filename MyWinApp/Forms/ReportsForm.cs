using RestaurantOrderManagement.Models;
using RestaurantOrderManagement.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderManagement
{
    public partial class ReportsForm : Form
    {
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private ListView _salesListView;
        private ListView _topItemsListView;
        private Label _totalRevenueLabel;
        private Label _totalOrdersLabel;
        private Label _averageOrderValueLabel;
        private ComboBox _dateRangeComboBox;
        private DateTimePicker _startDatePicker;
        private DateTimePicker _endDatePicker;

        public ReportsForm(OrderService orderService, PaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            InitializeComponent();
            InitializeUI();
            LoadReports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "ReportsForm";
            this.Text = "Reports & Analytics";
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
                Text = "Reports",
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

            // Left panel - Sales/Top Items
            var leftPanel = CreateLeftPanel();
            mainSplitContainer.Panel1.Controls.Add(leftPanel);

            // Right panel - Details/Charts
            var rightPanel = CreateRightPanel();
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(titleLabel);
            this.Controls.Add(logoPanel);
        }

        private Panel CreateDateRangePanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            var dateRangeLabel = new Label { Text = "Date Range:", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _dateRangeComboBox = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _dateRangeComboBox.Items.AddRange(new object[] { "Today", "Yesterday", "Last 7 Days", "Last 30 Days", "This Month", "Custom Range" });
            _dateRangeComboBox.SelectedIndex = 0;
            _dateRangeComboBox.SelectedIndexChanged += DateRangeComboBox_SelectedIndexChanged;

            var startDateLabel = new Label { Text = "Start Date:", Font = new Font("Segoe UI", 10) };
            _startDatePicker = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
            _startDatePicker.Value = DateTime.Today;

            var endDateLabel = new Label { Text = "End Date:", Font = new Font("Segoe UI", 10) };
            _endDatePicker = new DateTimePicker { Width = 150, Format = DateTimePickerFormat.Short };
            _endDatePicker.Value = DateTime.Today;

            var refreshButton = new Button
            {
                Text = "Refresh",
                Width = 80,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.Click += RefreshButton_Click;

            layout.Controls.AddRange(new Control[] { 
                dateRangeLabel, _dateRangeComboBox, 
                startDateLabel, _startDatePicker,
                endDateLabel, _endDatePicker,
                refreshButton 
            });

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreateStatsPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            // Total Revenue
            var revenuePanel = CreateStatBox("Total Revenue", "$0.00", Color.FromArgb(46, 204, 113), ref _totalRevenueLabel);
            layout.Controls.Add(revenuePanel, 0, 0);

            // Total Orders
            var ordersPanel = CreateStatBox("Total Orders", "0", Color.FromArgb(52, 152, 219), ref _totalOrdersLabel);
            layout.Controls.Add(ordersPanel, 1, 0);

            // Average Order Value
            var avgPanel = CreateStatBox("Average Order", "$0.00", Color.FromArgb(155, 89, 182), ref _averageOrderValueLabel);
            layout.Controls.Add(avgPanel, 2, 0);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreateStatBox(string title, string value, Color color, ref Label valueLabel)
        {
            var panel = new Panel
            {
                BackColor = color,
                Margin = new Padding(5),
                Padding = new Padding(10)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateSalesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var titleLabel = new Label
            {
                Text = "Sales Report",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _salesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };

            _salesListView.Columns.Add("Order ID", 80);
            _salesListView.Columns.Add("Date", 100);
            _salesListView.Columns.Add("Type", 80);
            _salesListView.Columns.Add("Customer", 120);
            _salesListView.Columns.Add("Items", 100);
            _salesListView.Columns.Add("Total", 100);
            _salesListView.Columns.Add("Status", 80);

            panel.Controls.Add(_salesListView);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateTopItemsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var titleLabel = new Label
            {
                Text = "Top Selling Items",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _topItemsListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };

            _topItemsListView.Columns.Add("Rank", 50);
            _topItemsListView.Columns.Add("Item", 150);
            _topItemsListView.Columns.Add("Quantity", 80);
            _topItemsListView.Columns.Add("Revenue", 100);

            panel.Controls.Add(_topItemsListView);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(34, 34, 40) };
            var salesPanel = CreateSalesPanel();
            salesPanel.Dock = DockStyle.Fill;
            panel.Controls.Add(salesPanel);
            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(34, 34, 40) };
            var topItemsPanel = CreateTopItemsPanel();
            topItemsPanel.Dock = DockStyle.Fill;
            panel.Controls.Add(topItemsPanel);
            return panel;
        }

        private void LoadReports()
        {
            var startDate = _startDatePicker.Value.Date;
            var endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1);

            LoadSalesReport(startDate, endDate);
            LoadTopItems(startDate, endDate);
            UpdateStatistics(startDate, endDate);
        }

        private void LoadSalesReport(DateTime startDate, DateTime endDate)
        {
            _salesListView.Items.Clear();
            var orders = _orderService.GetAllOrders()
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate && o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.OrderTime)
                .ToList();

            foreach (var order in orders)
            {
                var item = new ListViewItem(order.OrderId.ToString());
                item.SubItems.Add(order.OrderTime.ToString("MM/dd/yyyy"));
                item.SubItems.Add(order.Type.ToString());
                item.SubItems.Add(order.CustomerName);
                item.SubItems.Add(order.Items.Count.ToString());
                item.SubItems.Add($"${order.Items.Sum(i => i.Price * i.Quantity):F2}");
                item.SubItems.Add(order.Status.ToString());
                _salesListView.Items.Add(item);
            }
        }

        private void LoadTopItems(DateTime startDate, DateTime endDate)
        {
            _topItemsListView.Items.Clear();
            var orders = _orderService.GetAllOrders()
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate && o.Status == OrderStatus.Completed)
                .ToList();

                            var itemStats = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            for (int i = 0; i < itemStats.Count; i++)
            {
                var stat = itemStats[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(stat.Name);
                item.SubItems.Add(stat.Quantity.ToString());
                item.SubItems.Add($"${stat.Revenue:F2}");
                _topItemsListView.Items.Add(item);
            }
        }

        private void UpdateStatistics(DateTime startDate, DateTime endDate)
        {
            var orders = _orderService.GetAllOrders()
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate && o.Status == OrderStatus.Completed)
                .ToList();

            var totalRevenue = _paymentService.GetTotalRevenueByDateRange(startDate, endDate);
            var totalOrders = orders.Count;
            var averageOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            _totalRevenueLabel.Text = $"${totalRevenue:F2}";
            _totalOrdersLabel.Text = totalOrders.ToString();
            _averageOrderValueLabel.Text = $"${averageOrder:F2}";
        }

        private void DateRangeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var today = DateTime.Today;
            switch (_dateRangeComboBox.SelectedIndex)
            {
                case 0: // Today
                    _startDatePicker.Value = today;
                    _endDatePicker.Value = today;
                    break;
                case 1: // Yesterday
                    _startDatePicker.Value = today.AddDays(-1);
                    _endDatePicker.Value = today.AddDays(-1);
                    break;
                case 2: // Last 7 Days
                    _startDatePicker.Value = today.AddDays(-7);
                    _endDatePicker.Value = today;
                    break;
                case 3: // Last 30 Days
                    _startDatePicker.Value = today.AddDays(-30);
                    _endDatePicker.Value = today;
                    break;
                case 4: // This Month
                    _startDatePicker.Value = new DateTime(today.Year, today.Month, 1);
                    _endDatePicker.Value = today;
                    break;
                case 5: // Custom Range
                    // Keep current values
                    break;
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadReports();
        }

        // Add this helper class for rounded corners
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }
    }
} 