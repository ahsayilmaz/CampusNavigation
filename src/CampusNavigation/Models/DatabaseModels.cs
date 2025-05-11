using System.ComponentModel.DataAnnotations;

namespace CampusNavigation.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Password { get; set; }
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string? Name { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public string? Description { get; set; }
    }
}