using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Practitioner
    {
        public long Id { get; set; }
        public long? RelationshipId { get; set; }
        public long? OrganizationId { get; set; }
        public long? ContactId { get; set; }
        public long? AddressId { get; set; }
        public string DisplayName { get; set; }
        public string License { get; set; }
        public string Type { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual Address Address { get; set; }
        public virtual Contact Contact { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Relationship Relationship { get; set; }
    }
}
