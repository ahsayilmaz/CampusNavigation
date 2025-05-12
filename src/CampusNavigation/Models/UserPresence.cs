using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusNavigation.Models
{
    public class UserPresence
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; } 

        public int? CurrentBuildingId { get; set; } 

        [ForeignKey("CurrentBuildingId")]
        public Building? CurrentBuilding { get; set; } 

        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}
