using RestaurantOrderManagement.Models;
using RestaurantOrderManagement.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderManagement
{
    public partial class DineInForm : Form
    {
        private readonly OrderService _orderService;
        private readonly MenuService _menuService;
        private Order? _currentOrder;
        private ListView _ordersListView;
        private ListView _menuListView;
        private ListView _orderItemsListView;
        private TextBox _customerNameTextBox;
        private TextBox _tableNumberTextBox;
        private Label _totalLabel;
        private ComboBox _categoryComboBox;

        public DineInForm(OrderService orderService, MenuService menuService)
        {
            _orderService = orderService;
            _menuService = menuService;
            InitializeComponent();
            InitializeUI();
            LoadOrders();
            LoadMenu();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "DineInForm";
            this.Text = "Dine-In Orders";
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
                Text = "Dine-In Orders",
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

            // Left panel - Orders and customer info
            var leftPanel = CreateLeftPanel();
            mainSplitContainer.Panel1.Controls.Add(leftPanel);

            // Right panel - Menu and order items
            var rightPanel = CreateRightPanel();
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(titleLabel);
            this.Controls.Add(logoPanel);
        }

        private Panel CreateLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Title
            var titleLabel = new Label
            {
                Text = "Dine-In Orders",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Customer info group
            var customerGroup = new GroupBox
            {
                Text = "Customer Information",
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10)
            };

            var customerNameLabel = new Label { Text = "Customer Name:", Location = new Point(10, 25) };
            _customerNameTextBox = new TextBox { Location = new Point(120, 22), Width = 200 };

            var tableNumberLabel = new Label { Text = "Table Number:", Location = new Point(10, 55) };
            _tableNumberTextBox = new TextBox { Location = new Point(120, 52), Width = 200 };

            var newOrderButton = new Button
            {
                Text = "New Order",
                Location = new Point(10, 85),
                Width = 100,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            newOrderButton.Click += NewOrderButton_Click;

            customerGroup.Controls.AddRange(new Control[] { customerNameLabel, _customerNameTextBox, tableNumberLabel, _tableNumberTextBox, newOrderButton });

            // Orders list
            var ordersGroup = new GroupBox
            {
                Text = "Active Orders",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _ordersListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };
            _ordersListView.Columns.Add("Order ID", 80);
            _ordersListView.Columns.Add("Customer", 120);
            _ordersListView.Columns.Add("Table", 60);
            _ordersListView.Columns.Add("Status", 80);
            _ordersListView.Columns.Add("Total", 80);
            _ordersListView.Columns.Add("Payment", 80);
            _ordersListView.SelectedIndexChanged += OrdersListView_SelectedIndexChanged;

            ordersGroup.Controls.Add(_ordersListView);

            panel.Controls.Add(ordersGroup);
            panel.Controls.Add(customerGroup);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Menu section
            var menuGroup = new GroupBox
            {
                Text = "Menu",
                Dock = DockStyle.Top,
                Height = 300,
                Padding = new Padding(10)
            };

            _categoryComboBox = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 30
            };
            _categoryComboBox.Items.AddRange(Enum.GetValues(typeof(Category)).Cast<object>().ToArray());
            _categoryComboBox.SelectedIndex = 0;
            _categoryComboBox.SelectedIndexChanged += CategoryComboBox_SelectedIndexChanged;

            _menuListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };
            _menuListView.Columns.Add("Item", 200);
            _menuListView.Columns.Add("Price", 80);
            _menuListView.Columns.Add("Category", 100);
            _menuListView.DoubleClick += MenuListView_DoubleClick;

            menuGroup.Controls.Add(_menuListView);
            menuGroup.Controls.Add(_categoryComboBox);

            // Order items section
            var orderItemsGroup = new GroupBox
            {
                Text = "Order Items",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
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

            var buttonsPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            var submitOrderButton = new Button
            {
                Text = "Submit Order",
                Dock = DockStyle.Right,
                Width = 120,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            submitOrderButton.Click += SubmitOrderButton_Click;

            _totalLabel = new Label
            {
                Text = "Total: $0.00",
                Dock = DockStyle.Left,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            buttonsPanel.Controls.Add(submitOrderButton);
            buttonsPanel.Controls.Add(_totalLabel);

            orderItemsGroup.Controls.Add(_orderItemsListView);
            orderItemsGroup.Controls.Add(buttonsPanel);

            panel.Controls.Add(orderItemsGroup);
            panel.Controls.Add(menuGroup);

            return panel;
        }

        private void LoadOrders()
        {
            _ordersListView.Items.Clear();
            var dineInOrders = _orderService.GetOrdersByType(OrderType.DineIn);
            
            foreach (var order in dineInOrders)
            {
                var item = new ListViewItem(order.OrderId.ToString());
                item.SubItems.Add(order.CustomerName);
                item.SubItems.Add(order.TableNumber);
                item.SubItems.Add(order.Status.ToString());
                item.SubItems.Add($"${order.Items.Sum(i => i.Price * i.Quantity):F2}");
                item.SubItems.Add(order.IsPaid ? "Paid" : "Unpaid");
                item.Tag = order;
                _ordersListView.Items.Add(item);
            }
        }

        private void LoadMenu()
        {
            _menuListView.Items.Clear();
            var category = (Category)_categoryComboBox.SelectedItem;
            var menuItems = _menuService.GetMenuItemsByCategory(category);

            foreach (var menuItem in menuItems)
            {
                var item = new ListViewItem(menuItem.Name);
                item.SubItems.Add($"${menuItem.Price:F2}");
                item.SubItems.Add(menuItem.Category.ToString());
                item.Tag = menuItem;
                _menuListView.Items.Add(item);
            }
        }

        private void NewOrderButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_customerNameTextBox.Text) || string.IsNullOrWhiteSpace(_tableNumberTextBox.Text))
            {
                MessageBox.Show("Please enter customer name and table number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentOrder = _orderService.CreateOrder(
                OrderType.DineIn,
                _customerNameTextBox.Text,
                _tableNumberTextBox.Text
            );

            _orderItemsListView.Items.Clear();
            UpdateTotal();
            LoadOrders();
            
            _customerNameTextBox.Clear();
            _tableNumberTextBox.Clear();
        }

        private void MenuListView_DoubleClick(object sender, EventArgs e)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please create a new order first.", "No Active Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_menuListView.SelectedItems.Count > 0)
            {
                var menuItem = (MenuItem)_menuListView.SelectedItems[0].Tag;
                AddItemToOrder(menuItem);
            }
        }

        private void AddItemToOrder(MenuItem menuItem)
        {
            // Check if item already exists in order
            var existingItem = _currentOrder!.Items.FirstOrDefault(i => i.Name == menuItem.Name);
            if (existingItem != null)
            {
                existingItem.Quantity++;
                UpdateOrderItemsList();
            }
            else
            {
                _orderService.AddItemToOrder(_currentOrder.OrderId, menuItem, 1);
                UpdateOrderItemsList();
            }
        }

        private void UpdateOrderItemsList()
        {
            _orderItemsListView.Items.Clear();
            if (_currentOrder != null)
            {
                foreach (var item in _currentOrder.Items)
                {
                                    var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(item.Quantity.ToString());
                listItem.SubItems.Add($"${item.Price:F2}");
                listItem.SubItems.Add($"${item.Price * item.Quantity:F2}");
                _orderItemsListView.Items.Add(listItem);
                }
            }
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            if (_currentOrder != null)
            {
                var total = _currentOrder.Items.Sum(item => item.Price * item.Quantity);
                _totalLabel.Text = $"Total: ${total:F2}";
            }
            else
            {
                _totalLabel.Text = "Total: $0.00";
            }
        }

        private void SubmitOrderButton_Click(object sender, EventArgs e)
        {
            if (_currentOrder == null || _currentOrder.Items.Count == 0)
            {
                MessageBox.Show("Please add items to the order before submitting.", "Empty Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _orderService.UpdateOrderStatus(_currentOrder.OrderId, OrderStatus.Confirmed);
                MessageBox.Show($"Order #{_currentOrder.OrderId} submitted successfully!", "Order Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _currentOrder = null;
                _orderItemsListView.Items.Clear();
                UpdateTotal();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting order: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OrdersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ordersListView.SelectedItems.Count > 0)
            {
                _currentOrder = (Order)_ordersListView.SelectedItems[0].Tag;
                UpdateOrderItemsList();
            }
        }

        private void CategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMenu();
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