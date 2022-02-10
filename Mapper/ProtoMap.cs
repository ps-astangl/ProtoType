using System;
using System.Linq;
using Context.Context.Models;
using CRISP.GRPC.ClinicalRelationship;

namespace ProtoApp.Mapper
{
    public static class ProtoMap
    {
        public static CRISP.GRPC.ClinicalRelationship.Practitioner ToGrpcPractitioner(this Context.Context.Models.Practitioner input)
        {
            if (input == null)
                return null;

            var licenseInformation = input.ToGrpcLicenseInformation();
            CRISP.GRPC.ClinicalRelationship.Practitioner practitioner = new CRISP.GRPC.ClinicalRelationship.Practitioner
            {
                Id = input.Id,
                Name = string.IsNullOrWhiteSpace(input.DisplayName) ? new Name
                {
                    DisplayName = input?.DisplayName?.StringOrEmpty()
                } : null,
                Address = input.Demographic?.ToGrpcAddress(),
                ContactInformation = input.Demographic?.ToGrpcContactInformation(),
                ProviderType = input?.MedicalSpeciality?.ToProviderType() ??
                               CRISP.GRPC.ClinicalRelationship.Practitioner.Types.ProviderType.None,
                PatientRelationshipId = input.RelationshipId.GetValueOrDefault(),
                LicenseInformation = { },
                OrganizationIds = {  }
            };
            if (licenseInformation != null)
            {
                practitioner.LicenseInformation.Add(licenseInformation);
            }


            if (input.OrganizationId != null)
            {
                practitioner.OrganizationIds.Add(input.OrganizationId.Value);
            }
            return practitioner;
        }
        public static CRISP.GRPC.ClinicalRelationship.Organization ToGrpcOrganization(this Context.Context.Models.Organization input)
        {
            if (input == null)
                return null;

            CRISP.GRPC.ClinicalRelationship.Organization organization =
                new CRISP.GRPC.ClinicalRelationship.Organization
                {
                    Id = input.Id,
                    DataSource = input.Relationship?.DataSource?.StringOrEmpty(),
                    Name = input.ParticipantName?.StringOrEmpty(),
                    Source = input.AssigningAuthorityCode?.StringOrEmpty(),
                    SubstanceUseDisclosure = input.SubstancesUseDisclosure.GetValueOrDefault(),
                    ContactInformation = input.Demographic?.ToGrpcContactInformation(),
                    Address = input.Demographic?.ToGrpcAddress(),
                    // NOTE: Organizational License not in the database
                    // organization.OrganizationLicenses = new RepeatedField<LicenseInformation>();
                    PatientRelationshipId = input.RelationshipId.GetValueOrDefault()
                };
            if (input?.OrganizationPrograms?.Count == 0) return organization;
            var programs = input
                ?.OrganizationPrograms
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcProgram())
                ?.ToList();
            organization.Programs.Add(programs);
            return organization;
        }
        private static CRISP.GRPC.ClinicalRelationship.Program ToGrpcProgram(
            this OrganizationProgram input)
        {
            if (input == null)
                return null;

            return new CRISP.GRPC.ClinicalRelationship.Program
            {
                Id = input.Id,
                Name = input.Name.StringOrEmpty(),
                Description = input.Description.StringOrEmpty(),
                ContactInformation = null, // TODO: Find Mapping
                Source = string.Empty, // TODO: Find mapping
                OrganizationId = input.OrganizationId.GetValueOrDefault(),
                Address = null // TODO: Find mapping
            };
        }

        private static CRISP.GRPC.ClinicalRelationship.Practitioner.Types.ProviderType ToProviderType(this string input)
        {
            if (input == null)
                return CRISP.GRPC.ClinicalRelationship.Practitioner.Types.ProviderType.None;
            var hasType = Enum.TryParse<CRISP.GRPC.ClinicalRelationship.Practitioner.Types.ProviderType>(input, true, out var licenseType);
            return hasType ? licenseType : CRISP.GRPC.ClinicalRelationship.Practitioner.Types.ProviderType.None;
        }
        private static LicenseInformation ToGrpcLicenseInformation(this Context.Context.Models.Practitioner input)
        {
            if (input == null)
                return null;

            var hasType = Enum.TryParse<LicenseInformation.Types.LicenseType>(input.LicenseType, true, out var licenseType);
            if (hasType)
                return new LicenseInformation
                {
                    LicenseNumber = input.License?.StringOrEmpty(),
                    LicenseType = licenseType
                };

            return null;
        }
        private static Address ToGrpcAddress(this Demographic input)
        {
            if (input == null || input.Id == 0)
                return null;

            Address address = new Address
            {
                City = input.City?.StringOrEmpty(),
                State = input.State?.StringOrEmpty(),
                Zip = input.Zip?.StringOrEmpty(),
                AddressLine1 = input.AddressLine1?.StringOrEmpty(),
                AddressLine2 = input.AddressLine2?.StringOrEmpty(),
            };
            return address;
        }
        private static ContactInformation ToGrpcContactInformation(this Demographic input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.PhoneNumber))
                return null;

            return new ContactInformation
            {
                PhoneNumber = input.PhoneNumber.StringOrEmpty(),
                PhoneType = Enum.TryParse<ContactInformation.Types.PhoneType>("None", true,
                    out var phoneType)
                    ? phoneType
                    : ContactInformation.Types.PhoneType.None,
                Email = input.Email?.StringOrEmpty()
            };
        }
        private static string StringOrEmpty(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
        }
    }
}