using Context.Context.Models;
using CRISP.GRPC.ClinicalRelationship;
using ProtoApp.GRPC;
using ProtoApp.Models.Mapping;

namespace ProtoApp.Models.DTO
{
    public class RelationshipDto
    {
        public long Id { get; set; }
        public OrganizationDto Organization { get; set; }
        public PractitionerDto Practitioner { get; set; }
        public ProgramDto Program { get; set; }
        public string DataSource { get; set; }
        public string Source { get; set; }
        public string Mrn { get; set; }

        public PatientRelationship ToGrpc()
        {
            return Mapper.ToPatientRelationship(this);
        }
    }
}