using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderManagement.Models
{
    public enum Category
    {
        Appetizers,
        MainCourse,
        Desserts,
        Beverages,
        Sides
    }

    public class MenuItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public Category Category { get; set; }
        public bool IsAvailable { get; set; } = true;
        
        [MaxLength(200)]
        public string ImagePath { get; set; } = string.Empty;
        public int PreparationTimeMinutes { get; set; } = 15;

        public override string ToString()
        {
            return $"{Name} - ${Price:F2}";
        }
    }
} 