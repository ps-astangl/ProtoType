using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Address
    {
        public Address()
        {
            Organizations = new HashSet<Organization>();
            Practitioners = new HashSet<Practitioner>();
        }

        public long Id { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<Practitioner> Practitioners { get; set; }
    }
}
