using RestaurantOrderManagement.Models;
using RestaurantOrderManagement.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderManagement
{
    public partial class MenuManagementForm : Form
    {
        private readonly MenuService _menuService;
        private ListView _menuListView;
        private MenuItem? _selectedItem;
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private TextBox _priceTextBox;
        private ComboBox _categoryComboBox;
        private NumericUpDown _prepTimeNumeric;
        private CheckBox _isAvailableCheckBox;
        private Button _addButton;
        private Button _updateButton;
        private Button _deleteButton;
        private Button _clearButton;

        public MenuManagementForm(MenuService menuService)
        {
            _menuService = menuService;
            InitializeComponent();
            InitializeUI();
            LoadMenuItems();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Name = "MenuManagementForm";
            this.Text = "Menu Management";
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
                Text = "Menu Management",
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
                SplitterDistance = 600,
                BackColor = Color.FromArgb(28, 28, 34)
            };

            // Left panel - Menu items list
            var leftPanel = CreateMenuListPanel();
            mainSplitContainer.Panel1.Controls.Add(leftPanel);

            // Right panel - Edit form
            var rightPanel = CreateEditFormPanel();
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(titleLabel);
            this.Controls.Add(logoPanel);
        }

        private Panel CreateMenuListPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var titleLabel = new Label
            {
                Text = "Menu Items",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _menuListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };

            _menuListView.Columns.Add("ID", 50);
            _menuListView.Columns.Add("Name", 150);
            _menuListView.Columns.Add("Category", 100);
            _menuListView.Columns.Add("Price", 80);
            _menuListView.Columns.Add("Prep Time", 80);
            _menuListView.Columns.Add("Available", 80);
            _menuListView.Columns.Add("Description", 200);

            _menuListView.SelectedIndexChanged += MenuListView_SelectedIndexChanged;

            panel.Controls.Add(_menuListView);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateEditFormPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var titleLabel = new Label
            {
                Text = "Edit Menu Item",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var formPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Name
            var nameLabel = new Label { Text = "Name:", Location = new Point(10, 20), Width = 100 };
            _nameTextBox = new TextBox { Location = new Point(120, 17), Width = 250 };

            // Description
            var descLabel = new Label { Text = "Description:", Location = new Point(10, 50), Width = 100 };
            _descriptionTextBox = new TextBox { Location = new Point(120, 47), Width = 250, Height = 60, Multiline = true };

            // Price
            var priceLabel = new Label { Text = "Price ($):", Location = new Point(10, 120), Width = 100 };
            _priceTextBox = new TextBox { Location = new Point(120, 117), Width = 100 };

            // Category
            var categoryLabel = new Label { Text = "Category:", Location = new Point(10, 150), Width = 100 };
            _categoryComboBox = new ComboBox { Location = new Point(120, 147), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _categoryComboBox.Items.AddRange(Enum.GetValues(typeof(Category)).Cast<object>().ToArray());

            // Preparation Time
            var prepTimeLabel = new Label { Text = "Prep Time (min):", Location = new Point(10, 180), Width = 100 };
            _prepTimeNumeric = new NumericUpDown { Location = new Point(120, 177), Width = 100, Minimum = 1, Maximum = 120, Value = 15 };

            // Available
            _isAvailableCheckBox = new CheckBox { Text = "Available", Location = new Point(120, 210), Width = 100, Checked = true };

            // Buttons
            var buttonPanel = new Panel { Location = new Point(10, 250), Width = 360, Height = 40 };

            _addButton = new Button
            {
                Text = "Add New",
                Location = new Point(0, 0),
                Width = 80,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _addButton.Click += AddButton_Click;

            _updateButton = new Button
            {
                Text = "Update",
                Location = new Point(90, 0),
                Width = 80,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _updateButton.Click += UpdateButton_Click;

            _deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(180, 0),
                Width = 80,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _deleteButton.Click += DeleteButton_Click;

            _clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(270, 0),
                Width = 80,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _clearButton.Click += ClearButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { _addButton, _updateButton, _deleteButton, _clearButton });

            formPanel.Controls.AddRange(new Control[] {
                nameLabel, _nameTextBox,
                descLabel, _descriptionTextBox,
                priceLabel, _priceTextBox,
                categoryLabel, _categoryComboBox,
                prepTimeLabel, _prepTimeNumeric,
                _isAvailableCheckBox,
                buttonPanel
            });

            panel.Controls.Add(formPanel);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void LoadMenuItems()
        {
            _menuListView.Items.Clear();
            var menuItems = _menuService.GetAllMenuItemsForManagement();

            foreach (var item in menuItems)
            {
                var listItem = new ListViewItem(item.Id.ToString());
                listItem.SubItems.Add(item.Name);
                listItem.SubItems.Add(item.Category.ToString());
                listItem.SubItems.Add($"${item.Price:F2}");
                listItem.SubItems.Add($"{item.PreparationTimeMinutes} min");
                listItem.SubItems.Add(item.IsAvailable ? "Yes" : "No");
                listItem.SubItems.Add(item.Description);
                listItem.Tag = item;
                _menuListView.Items.Add(listItem);
            }
        }

        private void MenuListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_menuListView.SelectedItems.Count > 0)
            {
                _selectedItem = (MenuItem)_menuListView.SelectedItems[0].Tag;
                LoadItemToForm(_selectedItem);
                _updateButton.Enabled = true;
                _deleteButton.Enabled = true;
                _addButton.Enabled = false;
            }
            else
            {
                ClearForm();
                _selectedItem = null;
                _updateButton.Enabled = false;
                _deleteButton.Enabled = false;
                _addButton.Enabled = true;
            }
        }

        private void LoadItemToForm(MenuItem item)
        {
            _nameTextBox.Text = item.Name;
            _descriptionTextBox.Text = item.Description;
            _priceTextBox.Text = item.Price.ToString("F2");
            _categoryComboBox.SelectedItem = item.Category;
            _prepTimeNumeric.Value = item.PreparationTimeMinutes;
            _isAvailableCheckBox.Checked = item.IsAvailable;
        }

        private void ClearForm()
        {
            _nameTextBox.Clear();
            _descriptionTextBox.Clear();
            _priceTextBox.Clear();
            _categoryComboBox.SelectedIndex = -1;
            _prepTimeNumeric.Value = 15;
            _isAvailableCheckBox.Checked = true;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                var newItem = new MenuItem
                {
                    Name = _nameTextBox.Text.Trim(),
                    Description = _descriptionTextBox.Text.Trim(),
                    Price = decimal.Parse(_priceTextBox.Text),
                    Category = (Category)_categoryComboBox.SelectedItem,
                    PreparationTimeMinutes = (int)_prepTimeNumeric.Value,
                    IsAvailable = _isAvailableCheckBox.Checked
                };

                if (_menuService.ItemExists(newItem.Name))
                {
                    MessageBox.Show("A menu item with this name already exists.", "Duplicate Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    _menuService.AddMenuItem(newItem);
                    LoadMenuItems();
                    ClearForm();
                    MessageBox.Show("Menu item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding menu item: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null || !ValidateForm())
                return;

            var originalName = _selectedItem.Name;
            var newName = _nameTextBox.Text.Trim();

            if (newName != originalName && _menuService.ItemExists(newName, _selectedItem.Id))
            {
                MessageBox.Show("A menu item with this name already exists.", "Duplicate Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedItem.Name = newName;
            _selectedItem.Description = _descriptionTextBox.Text.Trim();
            _selectedItem.Price = decimal.Parse(_priceTextBox.Text);
            _selectedItem.Category = (Category)_categoryComboBox.SelectedItem;
            _selectedItem.PreparationTimeMinutes = (int)_prepTimeNumeric.Value;
            _selectedItem.IsAvailable = _isAvailableCheckBox.Checked;

            try
            {
                if (_menuService.UpdateMenuItem(_selectedItem))
                {
                    LoadMenuItems();
                    ClearForm();
                    _selectedItem = null;
                    _updateButton.Enabled = false;
                    _deleteButton.Enabled = false;
                    _addButton.Enabled = true;
                    MessageBox.Show("Menu item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update menu item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating menu item: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedItem.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_menuService.RemoveMenuItem(_selectedItem.Id))
                    {
                        LoadMenuItems();
                        ClearForm();
                        _selectedItem = null;
                        _updateButton.Enabled = false;
                        _deleteButton.Enabled = false;
                        _addButton.Enabled = true;
                        MessageBox.Show("Menu item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete menu item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting menu item: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearForm();
            _menuListView.SelectedItems.Clear();
            _selectedItem = null;
            _updateButton.Enabled = false;
            _deleteButton.Enabled = false;
            _addButton.Enabled = true;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the menu item.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(_priceTextBox.Text, out var price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _priceTextBox.Focus();
                return false;
            }

            if (_categoryComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _categoryComboBox.Focus();
                return false;
            }

            return true;
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