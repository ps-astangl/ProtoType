using System;
using System.Collections.Generic;

#nullable disable

namespace ProtoApp.Context.Models
{
    public partial class OrganizationProgram
    {
        public long Id { get; set; }
        public long? OrganizationId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public virtual Organization Organization { get; set; }
    }
}
