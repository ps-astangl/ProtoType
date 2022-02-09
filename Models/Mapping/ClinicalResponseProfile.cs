using System;
using CRISP.GRPC.ClinicalRelationship;
using ProtoApp.Models.DTO;

namespace ProtoApp.Models.Mapping
{
    public static class Mapper
    {
        public static CRISP.GRPC.ClinicalRelationship.Practitioner ToPractitioner(PractitionerDto input)
        {
            if (input == null)
                return null;

            var licenseInformation = input.LicenseInformation.ToLicenseInformation();
            Practitioner practitioner = new Practitioner
            {
                Id = input.Id,
                PatientRelationshipId = input.RelationshipId,
                Name = input.Name.ToGrpc(),
                Address = input.Demographics.ToAddress(),
                ContactInformation = input.Demographics.ToContactInformation(),
                LicenseInformation = { },
                OrganizationIds = { },
                ProviderType = Enum.TryParse<Practitioner.Types.ProviderType>(input.MedicalSpeciality, true,
                    out var providerType)
                    ? providerType
                    : Practitioner.Types.ProviderType.Unknown,
            };

            if (licenseInformation != null)
                practitioner.LicenseInformation.Add(licenseInformation);

            if (input.OrganizationId.HasValue)
                practitioner.OrganizationIds.Add(input.OrganizationId.Value);

            return practitioner;
        }
        public static CRISP.GRPC.ClinicalRelationship.Organization ToOrganization(OrganizationDto input)
        {
            if (input == null)
                return null;

            Organization organization = new Organization
            {
                Id = input.Id,
                Name = input.Name.StringOrEmpty(),
                Source = input.Source.StringOrEmpty(),
                DataSource = input.DataSource.StringOrEmpty(),
                SubstanceUseDisclosure = input.SubstanceUseDisclosure.GetValueOrDefault(),
                Address = input.Demographics?.ToAddress(),
                ContactInformation = input?.Demographics?.ToContactInformation(),
                PatientRelationshipId = input.RelationshipId
            };
            return organization;
        }
        private static CRISP.GRPC.ClinicalRelationship.Address ToAddress(this DemographicsDto input)
        {
            if (input == null)
                return null;

            Address address = new Address
            {
                City = input.City.StringOrEmpty(),
                State = input.State.StringOrEmpty(),
                Zip = input.Zip.StringOrEmpty(),
                AddressLine1 = input.AddressLine1.StringOrEmpty(),
                AddressLine2 = input.AddressLine2.StringOrEmpty(),
            };
            return address;
        }
        private static CRISP.GRPC.ClinicalRelationship.ContactInformation ToContactInformation(this DemographicsDto input)
        {
            if (string.IsNullOrWhiteSpace(input.PhoneNumber))
                return null;

            return new ContactInformation
            {
                PhoneNumber = input.PhoneNumber.StringOrEmpty(),
                PhoneType = Enum.TryParse<ContactInformation.Types.PhoneType>(input.PhoneType, true,
                    out var phoneType)
                    ? phoneType
                    : ContactInformation.Types.PhoneType.None,
                Email = input.Email.StringOrEmpty()
            };
        }
        public static CRISP.GRPC.ClinicalRelationship.Program ToProgram(this ProgramDto programDto, long? organizationId)
        {
            if (programDto == null)
                return null;

            return new CRISP.GRPC.ClinicalRelationship.Program
            {
                Id = programDto.Id,
                Name = programDto.Name.StringOrEmpty(),
                Description = programDto.Description.StringOrEmpty(),
                ContactInformation = null, // TODO: Find Mapping
                Source = string.Empty, // TODO: Find mapping
                OrganizationId = organizationId.GetValueOrDefault()
            };
        }
        public static CRISP.GRPC.ClinicalRelationship.Name ToName(NameDto input)
        {
            if (input == null)
                return null;

            Name name = new Name
            {
                FirstName = input.Firstname.StringOrEmpty(),
                MiddleName = input.MiddleName.StringOrEmpty(),
                LastName = input.LastName.StringOrEmpty(),
                DisplayName = input.DisplayName.StringOrEmpty()
            };
            return name;
        }
        private static CRISP.GRPC.ClinicalRelationship.LicenseInformation ToLicenseInformation(this LicenseInformationDto input)
        {
            if (input == null)
                return null;

            var hasType = Enum.TryParse<LicenseInformation.Types.LicenseType>(input.Type, true, out var licenseType);
            if (hasType)
                return new LicenseInformation
                {
                    LicenseNumber = input.Value.StringOrEmpty(),
                    LicenseType = licenseType
                };

            return null;
        }
        private static string StringOrEmpty(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
        }
        public static PatientRelationship ToPatientRelationship(RelationshipDto relationshipDto)
        {
            PatientRelationship patientRelationship = new PatientRelationship();

            if (relationshipDto == null)
                return null;
            return patientRelationship;
        }
    }
}