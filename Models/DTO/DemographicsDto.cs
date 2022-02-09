using System;
using CRISP.GRPC.ClinicalRelationship;

namespace ProtoApp.Models.DTO
{

    public class DemographicsDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string PhoneType { get; set; } = "None";
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public long? Id { get; set; }
    }
}