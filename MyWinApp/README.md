# Restaurant Order Management System

A comprehensive desktop application built with C# WinForms for managing restaurant operations including dine-in orders, takeout orders, kitchen management, and reporting.

## Features

### 🍽️ Dine-In Management
- Create and manage dine-in orders
- Assign tables to customers
- Real-time order tracking
- Easy menu item selection
- Order status updates

### 📦 Takeout Management
- Handle takeout orders efficiently
- Customer information management
- Pickup time scheduling
- Order confirmation system

### 👨‍🍳 Kitchen Dashboard
- Real-time order monitoring
- Order status management (Pending → In Progress → Ready)
- Item-level preparation tracking
- Kitchen staff workflow optimization
- Auto-refresh every 5 seconds

### 📊 Reports & Analytics
- Sales reports with date range filtering
- Revenue tracking
- Top-selling items analysis
- Order statistics
- Performance metrics

## System Architecture

### Models
- **Order**: Core order entity with status tracking
- **OrderItem**: Individual items within orders
- **MenuItem**: Menu items with categories and pricing

### Services
- **OrderService**: Business logic for order management
- **MenuService**: Menu item management and sample data

### Forms
- **MainForm**: Central navigation hub
- **DineInForm**: Dine-in order management
- **TakeoutForm**: Takeout order handling
- **KitchenForm**: Kitchen staff dashboard
- **ReportsForm**: Analytics and reporting

## Getting Started

### Prerequisites
- .NET 9.0 or later
- Windows operating system
- Visual Studio 2022 or VS Code

### Installation
1. Clone or download the project
2. Open the solution in Visual Studio
3. Build the project (Ctrl+Shift+B)
4. Run the application (F5)

### Usage

#### Starting the Application
1. Launch the application
2. You'll see the main dashboard with four modules
3. Click on any module to access its functionality

#### Creating Orders
1. **Dine-In Orders**:
   - Enter customer name and table number
   - Click "New Order"
   - Double-click menu items to add to order
   - Submit when complete

2. **Takeout Orders**:
   - Enter customer name and phone number
   - Set pickup time
   - Add menu items
   - Submit order

#### Kitchen Management
1. Open Kitchen Dashboard
2. View orders by status (Pending, In Progress, Ready)
3. Double-click orders to see details
4. Update order status as items are prepared
5. Mark orders as complete when served

#### Reports
1. Open Reports & Analytics
2. Select date range
3. View sales data and top items
4. Refresh data as needed

## Menu Categories

The system includes a comprehensive menu with the following categories:
- **Appetizers**: Starters and small plates
- **Main Course**: Entrees and primary dishes
- **Sides**: Accompaniments and side dishes
- **Desserts**: Sweet endings
- **Beverages**: Drinks and refreshments

## Order Status Flow

1. **Pending**: Order created, awaiting confirmation
2. **Confirmed**: Order confirmed, ready for kitchen
3. **InKitchen**: Order being prepared
4. **Ready**: Order ready for pickup/serving
5. **Served**: Order delivered to customer
6. **Completed**: Order finished
7. **Cancelled**: Order cancelled

## Technical Details

### Technology Stack
- **Framework**: .NET 9.0
- **UI**: Windows Forms (WinForms)
- **Language**: C#
- **Architecture**: Service-oriented with event-driven updates

### Key Features
- Real-time order updates
- Event-driven architecture
- Responsive UI design
- Comprehensive error handling
- Modular design for easy maintenance

### Data Management
- In-memory data storage (can be extended to database)
- Sample menu data included
- Order persistence during application session

## Customization

### Adding Menu Items
Edit the `MenuService.cs` file to add or modify menu items:

```csharp
new MenuItem { 
    Id = 21, 
    Name = "New Item", 
    Description = "Description", 
    Price = 15.99m, 
    Category = Category.MainCourse, 
    PreparationTimeMinutes = 20 
}
```

### Modifying Order Statuses
Update the `OrderStatus` enum in `Order.cs` to add new statuses or modify existing ones.

### UI Customization
All forms use a consistent color scheme and styling that can be modified in the respective form files.

## Future Enhancements

Potential improvements for future versions:
- Database integration (SQL Server, SQLite)
- User authentication and roles
- Inventory management
- Payment processing
- Customer loyalty system
- Mobile app integration
- Advanced analytics and forecasting
- Multi-location support

## Support

For questions or issues:
1. Check the code comments for implementation details
2. Review the service classes for business logic
3. Examine the form files for UI behavior

## License

This project is provided as-is for educational and commercial use.

---

**Restaurant Order Management System** - Streamlining restaurant operations with modern desktop technology. 