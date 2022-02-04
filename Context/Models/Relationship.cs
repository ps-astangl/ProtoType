using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Relationship
    {
        public Relationship()
        {
            Organizations = new HashSet<Organization>();
            Practitioners = new HashSet<Practitioner>();
        }

        public long Id { get; set; }
        public long? PatientId { get; set; }
        public string DataSource { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<Practitioner> Practitioners { get; set; }
    }
}
