using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class Organization
    {
        public Organization()
        {
            OrganizationPrograms = new HashSet<OrganizationProgram>();
            Practitioners = new HashSet<Practitioner>();
        }

        public long Id { get; set; }
        public long? RelationshipId { get; set; }
        public long? ContactId { get; set; }
        public long? AddressId { get; set; }
        public string ParticipantSourceCode { get; set; }
        public string AssigningAuthorityCode { get; set; }
        public string ParticipantName { get; set; }
        public bool? SubstancesUseDisclosure { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual Address Address { get; set; }
        public virtual Contact Contact { get; set; }
        public virtual Relationship Relationship { get; set; }
        public virtual ICollection<OrganizationProgram> OrganizationPrograms { get; set; }
        public virtual ICollection<Practitioner> Practitioners { get; set; }
    }
}
