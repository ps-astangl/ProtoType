using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Contact
    {
        public Contact()
        {
            Organizations = new HashSet<Organization>();
            Practitioners = new HashSet<Practitioner>();
        }

        public long Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<Practitioner> Practitioners { get; set; }
    }
}
