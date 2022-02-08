using CRISP.GRPC.ClinicalRelationship;
using ProtoApp.Models.Mapping;

public class LicenseInformationDto
{
    public string Type { get; set; }
    public string Value { get; set; }
    public LicenseInformation ToGrpc()
    {
        return Mapper.MapLicenseInformation(this);
    }
}