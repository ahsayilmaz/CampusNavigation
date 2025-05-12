using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusNavigation.Models
{
    public class BuildingConnection
    {
        public int Id { get; set; }
        
        [ForeignKey("FromBuilding")]
        public int FromBuildingId { get; set; }
        
        [ForeignKey("ToBuilding")]
        public int ToBuildingId { get; set; }
        
        public virtual Building? FromBuilding { get; set; } 
        public virtual Building? ToBuilding { get; set; } 
        
        public int Distance { get; set; }
        public double TrafficFactor { get; set; }
    }
}