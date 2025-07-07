using RestaurantOrderManagement.Models;
using RestaurantOrderManagement.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderManagement
{
    public partial class KitchenForm : Form
    {
        private readonly OrderService _orderService;
        private ListView _pendingOrdersListView;
        private ListView _inProgressOrdersListView;
        private ListView _readyOrdersListView;
        private System.Windows.Forms.Timer _refreshTimer;
        private Label _totalOrdersLabel;
        private Label _pendingCountLabel;
        private Label _inProgressCountLabel;
        private Label _readyCountLabel;

        public KitchenForm(OrderService orderService)
        {
            _orderService = orderService;
            InitializeComponent();
            InitializeUI();
            SetupTimer();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 900);
            this.Name = "KitchenForm";
            this.Text = "Kitchen Dashboard";
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
                Text = "Kitchen Dashboard",
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

            // Left panel - Pending/In Progress/Ready Orders
            var leftPanel = CreateLeftPanel();
            mainSplitContainer.Panel1.Controls.Add(leftPanel);

            // Right panel - Order details
            var rightPanel = CreateRightPanel();
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(titleLabel);
            this.Controls.Add(logoPanel);
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
                ColumnCount = 4,
                RowCount = 1
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // Total Orders
            var totalPanel = CreateStatBox("Total Orders", "0", Color.FromArgb(155, 89, 182), ref _totalOrdersLabel);
            layout.Controls.Add(totalPanel, 0, 0);

            // Pending Orders
            var pendingPanel = CreateStatBox("Pending", "0", Color.FromArgb(255, 193, 7), ref _pendingCountLabel);
            layout.Controls.Add(pendingPanel, 1, 0);

            // In Progress Orders
            var inProgressPanel = CreateStatBox("In Progress", "0", Color.FromArgb(52, 152, 219), ref _inProgressCountLabel);
            layout.Controls.Add(inProgressPanel, 2, 0);

            // Ready Orders
            var readyPanel = CreateStatBox("Ready", "0", Color.FromArgb(46, 204, 113), ref _readyCountLabel);
            layout.Controls.Add(readyPanel, 3, 0);

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
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateOrderColumn(string title, Color color, ref ListView listView)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = color,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            listView.Columns.Add("Order ID", 80);
            listView.Columns.Add("Type", 60);
            listView.Columns.Add("Customer", 100);
            listView.Columns.Add("Items", 150);
            listView.Columns.Add("Time", 80);
            listView.Columns.Add("Status", 80);

            listView.DoubleClick += OrderListView_DoubleClick;

            panel.Controls.Add(listView);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void SetupTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // Refresh every 5 seconds
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void LoadOrders()
        {
            LoadPendingOrders();
            LoadInProgressOrders();
            LoadReadyOrders();
            UpdateStatistics();
        }

        private void LoadPendingOrders()
        {
            _pendingOrdersListView.Items.Clear();
            var pendingOrders = _orderService.GetOrdersByStatus(OrderStatus.Confirmed);

            foreach (var order in pendingOrders)
            {
                var item = CreateOrderListViewItem(order);
                _pendingOrdersListView.Items.Add(item);
            }
        }

        private void LoadInProgressOrders()
        {
            _inProgressOrdersListView.Items.Clear();
            var inProgressOrders = _orderService.GetOrdersByStatus(OrderStatus.InKitchen);

            foreach (var order in inProgressOrders)
            {
                var item = CreateOrderListViewItem(order);
                _inProgressOrdersListView.Items.Add(item);
            }
        }

        private void LoadReadyOrders()
        {
            _readyOrdersListView.Items.Clear();
            var readyOrders = _orderService.GetOrdersByStatus(OrderStatus.Ready);

            foreach (var order in readyOrders)
            {
                var item = CreateOrderListViewItem(order);
                _readyOrdersListView.Items.Add(item);
            }
        }

        private ListViewItem CreateOrderListViewItem(Order order)
        {
            var item = new ListViewItem(order.OrderId.ToString());
            item.SubItems.Add(order.Type.ToString());
            item.SubItems.Add(order.CustomerName);
            item.SubItems.Add($"{order.Items.Count} items");
            item.SubItems.Add(order.OrderTime.ToString("HH:mm"));
            item.SubItems.Add(order.Status.ToString());
            item.Tag = order;
            return item;
        }

        private void UpdateStatistics()
        {
            var allOrders = _orderService.GetActiveOrders();
            var pendingCount = _orderService.GetOrdersByStatus(OrderStatus.Confirmed).Count;
            var inProgressCount = _orderService.GetOrdersByStatus(OrderStatus.InKitchen).Count;
            var readyCount = _orderService.GetOrdersByStatus(OrderStatus.Ready).Count;

            _totalOrdersLabel.Text = allOrders.Count.ToString();
            _pendingCountLabel.Text = pendingCount.ToString();
            _inProgressCountLabel.Text = inProgressCount.ToString();
            _readyCountLabel.Text = readyCount.ToString();
        }

        private void OrderListView_DoubleClick(object sender, EventArgs e)
        {
            var listView = (ListView)sender;
            if (listView.SelectedItems.Count > 0)
            {
                var order = (Order)listView.SelectedItems[0].Tag;
                ShowOrderDetails(order);
            }
        }

        private void ShowOrderDetails(Order order)
        {
            var detailsForm = new Form
            {
                Text = $"Order #{order.OrderId} Details",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Order info
            var infoLabel = new Label
            {
                Text = $"Order #{order.OrderId}\nCustomer: {order.CustomerName}\nType: {order.Type}\nStatus: {order.Status}\nOrder Time: {order.OrderTime:HH:mm}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 100
            };

            // Items list
            var itemsLabel = new Label { Text = "Items:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 20 };
            var itemsListBox = new ListBox { Dock = DockStyle.Fill };

            foreach (var item in order.Items)
            {
                itemsListBox.Items.Add($"{item.Quantity}x {item.Name} - ${item.Price * item.Quantity:F2}");
                if (!string.IsNullOrEmpty(item.SpecialInstructions))
                {
                    itemsListBox.Items.Add($"  Special: {item.SpecialInstructions}");
                }
            }

            // Buttons
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            if (order.Status == OrderStatus.Confirmed)
            {
                var startButton = new Button
                {
                    Text = "Start Preparation",
                    Dock = DockStyle.Left,
                    Width = 120,
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White
                };
                startButton.Click += (s, e) =>
                {
                    _orderService.UpdateOrderStatus(order.OrderId, OrderStatus.InKitchen);
                    detailsForm.Close();
                    LoadOrders();
                };
                buttonPanel.Controls.Add(startButton);
            }
            else if (order.Status == OrderStatus.InKitchen)
            {
                var readyButton = new Button
                {
                    Text = "Mark Ready",
                    Dock = DockStyle.Left,
                    Width = 120,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White
                };
                readyButton.Click += (s, e) =>
                {
                    _orderService.UpdateOrderStatus(order.OrderId, OrderStatus.Ready);
                    detailsForm.Close();
                    LoadOrders();
                };
                buttonPanel.Controls.Add(readyButton);
            }
            else if (order.Status == OrderStatus.Ready)
            {
                var completeButton = new Button
                {
                    Text = "Complete Order",
                    Dock = DockStyle.Left,
                    Width = 120,
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White
                };
                completeButton.Click += (s, e) =>
                {
                    _orderService.CompleteOrder(order.OrderId);
                    detailsForm.Close();
                    LoadOrders();
                };
                buttonPanel.Controls.Add(completeButton);
            }

            var closeButton = new Button
            {
                Text = "Close",
                Dock = DockStyle.Right,
                Width = 80
            };
            closeButton.Click += (s, e) => detailsForm.Close();
            buttonPanel.Controls.Add(closeButton);

            panel.Controls.Add(buttonPanel);
            panel.Controls.Add(itemsListBox);
            panel.Controls.Add(itemsLabel);
            panel.Controls.Add(infoLabel);

            detailsForm.Controls.Add(panel);
            detailsForm.ShowDialog();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadOrders();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }

        // Add this helper class for rounded corners
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }

        private Panel CreateLeftPanel()
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(34, 34, 40),
                Padding = new Padding(5)
            };
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

            var pendingPanel = CreateOrderColumn("Pending Orders", Color.FromArgb(255, 193, 7), ref _pendingOrdersListView);
            var inProgressPanel = CreateOrderColumn("In Progress", Color.FromArgb(52, 152, 219), ref _inProgressOrdersListView);
            var readyPanel = CreateOrderColumn("Ready for Pickup", Color.FromArgb(46, 204, 113), ref _readyOrdersListView);

            tableLayout.Controls.Add(pendingPanel, 0, 0);
            tableLayout.Controls.Add(inProgressPanel, 0, 1);
            tableLayout.Controls.Add(readyPanel, 0, 2);

            return tableLayout;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(34, 34, 40) };
            var statsPanel = CreateStatsPanel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 100;
            panel.Controls.Add(statsPanel);
            // Optionally add more details or a placeholder below
            return panel;
        }
    }
} 