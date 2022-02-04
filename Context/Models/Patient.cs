using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Patient
    {
        public Patient()
        {
            Relationships = new HashSet<Relationship>();
        }

        public long Id { get; set; }
        public string Eid { get; set; }
        public string Source { get; set; }
        public string Mrn { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual ICollection<Relationship> Relationships { get; set; }
    }
}
