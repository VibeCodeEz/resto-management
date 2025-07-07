using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagement.Models
{
    public enum OrderType
    {
        DineIn,
        Takeout
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        InKitchen,
        Ready,
        Served,
        Completed,
        Cancelled
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? EstimatedReadyTime { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string TableNumber { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        
        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;
        
        // Payment information
        public Payment? Payment { get; set; }
        public bool IsPaid => Payment?.Status == PaymentStatus.Completed;

        public Order()
        {
            OrderTime = DateTime.Now;
            Status = OrderStatus.Pending;
        }
    }

    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        
        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;
        public bool IsReady { get; set; }
    }
} 