﻿using CRISP.GRPC.ClinicalRelationship;

namespace ProtoApp.Models.DTO
{
    public class OrganizationDto
    {
        public long Id { get; set; }
        public string DataSource { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool? SubstanceUseDisclosure { get; set; } = false;
        public DemographicsDto Demographics { get; set; }
        public long RelationshipId { get; set; }

        public Organization ToGrpc()
        {
            return Mapping.Mapper.ToOrganization(this);
        }
    }
}