using System;
using System.ComponentModel.DataAnnotations;

namespace CampusNavigation.Models
{
    public class UserLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        public string? CurrentNode { get; set; }

        public string? CurrentEdge { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
