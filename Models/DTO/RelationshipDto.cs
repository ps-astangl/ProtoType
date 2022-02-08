using Context.Context.Models;
using ProtoApp.GRPC;

namespace ProtoApp.Models.DTO
{
    public class RelationshipDto
    {
        public long Id { get; set; }
        public OrganizationDto Organization { get; set; }
        public PractitionerDto Practitioner { get; set; }
        public ProgramDto Program { get; set; }
        public Relationship ToGrpc()
        {
            return new Relationship();
        }
    }
}