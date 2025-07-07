using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagement.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        
        public int OrderId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Change { get; set; }
        
        public PaymentStatus Status { get; set; }
        
        public DateTime PaymentTime { get; set; }
        
        [MaxLength(100)]
        public string CashierName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public string ReceiptNumber { get; set; } = string.Empty;
        
        // Navigation property
        public Order? Order { get; set; }
        
        public Payment()
        {
            PaymentTime = DateTime.Now;
            Status = PaymentStatus.Pending;
            ReceiptNumber = GenerateReceiptNumber();
        }
        
        private string GenerateReceiptNumber()
        {
            return $"R{DateTime.Now:yyyyMMdd}{DateTime.Now:HHmmss}";
        }
    }
} 