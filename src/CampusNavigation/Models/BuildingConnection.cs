using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusNavigation.Models
{
    public class BuildingConnection
    {
        public int Id { get; set; }
        
        // Explicitly identify these as foreign keys
        [ForeignKey("FromBuilding")]
        public int FromBuildingId { get; set; }
        
        [ForeignKey("ToBuilding")]
        public int ToBuildingId { get; set; }
        
        // Navigation properties
        public virtual Building? FromBuilding { get; set; } // Made nullable
        public virtual Building? ToBuilding { get; set; } // Made nullable
        
        public int Distance { get; set; }
        public double TrafficFactor { get; set; }
    }
}