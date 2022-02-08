using System;
using System.Collections.Generic;
using System.Linq;
using CRISP.GRPC.ClinicalRelationship;
using Google.Protobuf.Collections;
using ProtoApp.Models.DTO;

namespace ProtoApp.Models.Mapping
{
    public static class Mapper
    {
        public static Practitioner MapPractitioner(PractitionerDto input)
        {
            if (input == null)
                return null;

            var licenseInformation = MapLicenseInformation(input.LicenseInformation);
            Practitioner practitioner = new Practitioner
            {
                Id = input.Id,
                Name = input.Name.ToGrpc(),
                Address = input.Demographics.MapAddress(),
                ContactInformation = input.Demographics.MapContactInformation(),
                Type = Enum.TryParse<Practitioner.Types.ProviderType>(input.MedicalSpeciality, true, out var type) ?
                    type : Practitioner.Types.ProviderType.None,
                LicenseInformation = { }
            };
            if (licenseInformation != null)
                practitioner.LicenseInformation.Add(licenseInformation);

            return practitioner;
        }

        public static Organization MapOrganization(OrganizationDto input)
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
                Address = input.Demographics?.MapAddress(),
                ContactInformation = input?.Demographics?.MapContactInformation(),
            };
            return organization;
        }

        private static Address MapAddress(this DemographicsDto demographicsDto)
        {
            if (demographicsDto == null)
                return null;

            Address address = new Address
            {
                City = demographicsDto.City.StringOrEmpty(),
                State = demographicsDto.State.StringOrEmpty(),
                Zip = demographicsDto.Zip.StringOrEmpty(),
                AddressLine1 = demographicsDto.AddressLine1.StringOrEmpty(),
                AddressLine2 = demographicsDto.AddressLine2.StringOrEmpty(),
            };
            return address;
        }

        private static ContactInformation MapContactInformation(this DemographicsDto demographicsDto)
        {
            if (string.IsNullOrWhiteSpace(demographicsDto.PhoneNumber))
                return null;

            return new ContactInformation
            {
                Phonenumber = demographicsDto.PhoneNumber.StringOrEmpty(),
                Type = Enum.TryParse<ContactInformation.Types.PhoneType>(demographicsDto.PhoneType, true, out var phoneType) ? phoneType : ContactInformation.Types.PhoneType.None,
                Email = demographicsDto.Email.StringOrEmpty()
            };
        }

        public static CRISP.GRPC.ClinicalRelationship.Program MapProgram(ProgramDto programDto, long? organizationId)
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

        public static Name MapName(NameDto input)
        {
            if (input == null)
                return null;

            Name name = new Name
            {
                Firstname = input.Firstname.StringOrEmpty(),
                MiddleName = input.MiddleName.StringOrEmpty(),
                LastName = input.LastName.StringOrEmpty(),
                DisplayName = input.DisplayName.StringOrEmpty()
            };
            return name;
        }

        private static string StringOrEmpty(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();
        }

        public static LicenseInformation MapLicenseInformation(LicenseInformationDto input)
        {
            if (input == null)
                return null;

            var hasType = Enum.TryParse<LicenseInformation.Types.LicenseType>(input.Type, true, out var type);
            if (hasType)
                return new LicenseInformation
                {
                    LicenseNumber = input.Value.StringOrEmpty(),
                    Type = type
                };

            return null;
        }
    }
}