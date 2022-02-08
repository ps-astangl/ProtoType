namespace ProtoApp.Models.DTO
{
    public class ProgramDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? OrganizationId { get; set; }

        public CRISP.GRPC.ClinicalRelationship.Program ToGrpc()
        {
            return Mapping.Mapper.ToProgram(this, OrganizationId);
        }
    }
}