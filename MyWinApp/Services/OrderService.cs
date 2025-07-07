using RestaurantOrderManagement.Data;
using RestaurantOrderManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantOrderManagement.Services
{
    public class OrderService
    {
        private readonly RestaurantDbContext _context;

        public OrderService(RestaurantDbContext context)
        {
            _context = context;
        }

        public event EventHandler<Order>? OrderAdded;
        public event EventHandler<Order>? OrderUpdated;

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }

        public List<Order> GetOrdersByType(OrderType type)
        {
            return _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Type == type)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }

        public List<Order> GetOrdersByStatus(OrderStatus status)
        {
            return _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }

        public Order CreateOrder(OrderType type, string customerName, string tableNumber = "", string phoneNumber = "")
        {
            var order = new Order
            {
                Type = type,
                CustomerName = customerName,
                TableNumber = tableNumber,
                PhoneNumber = phoneNumber
            };

            _context.Orders.Add(order);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to create order: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            OrderAdded?.Invoke(this, order);
            return order;
        }

        public void AddItemToOrder(int orderId, MenuItem menuItem, int quantity, string specialInstructions = "")
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderId == orderId);
                
            if (order != null)
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ItemId = order.Items.Count + 1,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = quantity,
                    SpecialInstructions = specialInstructions
                };

                order.Items.Add(orderItem);
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to add item to order: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                OrderUpdated?.Invoke(this, order);
            }
        }

        public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = newStatus;
                if (newStatus == OrderStatus.InKitchen)
                {
                    var items = _context.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
                    order.EstimatedReadyTime = DateTime.Now.AddMinutes(
                        items.Sum(item => 15) // Default 15 minutes per item
                    );
                }
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to update order status: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                OrderUpdated?.Invoke(this, order);
            }
        }

        public void MarkItemReady(int orderId, int itemId)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderId == orderId);
                
            if (order != null)
            {
                var item = order.Items.FirstOrDefault(i => i.ItemId == itemId);
                if (item != null)
                {
                    item.IsReady = true;
                    
                    // Check if all items are ready
                    if (order.Items.All(i => i.IsReady))
                    {
                        order.Status = OrderStatus.Ready;
                    }
                    
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        throw new InvalidOperationException($"Failed to mark item ready: {ex.InnerException?.Message ?? ex.Message}", ex);
                    }
                    OrderUpdated?.Invoke(this, order);
                }
            }
        }

        public void CompleteOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Completed;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to complete order: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                OrderUpdated?.Invoke(this, order);
            }
        }

        public void CancelOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Cancelled;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to cancel order: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                OrderUpdated?.Invoke(this, order);
            }
        }

        public List<Order> GetActiveOrders()
        {
            return _context.Orders
                .Include(o => o.Items)
                .Where(o => 
                    o.Status != OrderStatus.Completed && 
                    o.Status != OrderStatus.Cancelled
                )
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }

        public decimal GetTotalRevenue()
        {
            return _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .Include(o => o.Items)
                .Sum(o => o.Items.Sum(item => item.Price * item.Quantity));
        }

        public decimal GetTotalRevenueForDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && 
                           o.OrderTime >= startDate && 
                           o.OrderTime <= endDate)
                .Include(o => o.Items)
                .Sum(o => o.Items.Sum(item => item.Price * item.Quantity));
        }
    }
} 