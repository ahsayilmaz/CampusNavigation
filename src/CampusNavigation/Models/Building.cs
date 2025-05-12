using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusNavigation.Models
{
    public class Building
    {
        public Building()
        {
            OutgoingConnections = new HashSet<BuildingConnection>();
            IncomingConnections = new HashSet<BuildingConnection>();
        }

        public int Id { get; set; }
        
        [MaxLength(200)]
        public string? Name { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public virtual ICollection<BuildingConnection> OutgoingConnections { get; set; }
        public virtual ICollection<BuildingConnection> IncomingConnections { get; set; }
    }
}
