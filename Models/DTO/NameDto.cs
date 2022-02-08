using CRISP.GRPC.ClinicalRelationship;

namespace ProtoApp.Models.DTO
{
    public class NameDto
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string DisplayName { get; set; }
    }
}