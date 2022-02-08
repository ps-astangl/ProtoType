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
        public static Organization MapOrganization(OrganizationDto input)
        {
            if (input == null)
                return null;

            Organization organization = new Organization
            {
                Id = input.Id,
                Name = input.Name,
                Source = input.Source,
                DataSource = input.DataSource,
                SubstanceUseDisclosure = input.SubstanceUseDisclosure.GetValueOrDefault(),
                Address = input?.Demographics?.MapAddress(),
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
                City = demographicsDto.City ?? string.Empty,
                State = demographicsDto.State ?? string.Empty,
                Zip = demographicsDto.Zip ?? string.Empty,
                AddressLine1 = demographicsDto.AddressLine1 ?? string.Empty,
                AddressLine2 = demographicsDto.AddressLine2 ?? string.Empty,
            };
            return address;
        }

        private static ContactInformation MapContactInformation(this DemographicsDto demographicsDto)
        {
            if (string.IsNullOrWhiteSpace(demographicsDto.PhoneNumber))
                return null;

            return new ContactInformation
            {
                Phonenumber = demographicsDto.PhoneNumber ?? string.Empty,
                Type = Enum.TryParse<ContactInformation.Types.PhoneType>(demographicsDto.PhoneType, true, out var phoneType) ? phoneType : ContactInformation.Types.PhoneType.None,
                Email = demographicsDto.Email ?? string.Empty
            };
        }

        public static CRISP.GRPC.ClinicalRelationship.Program MapProgram(ProgramDto programDto, long? organizationId)
        {
            if (programDto == null)
                return null;

            return new CRISP.GRPC.ClinicalRelationship.Program
            {
                Id = programDto.Id,
                Name = programDto.Name ?? string.Empty,
                Description = programDto.Description ?? string.Empty,
                ContactInformation = null, // TODO: Find Mapping
                Source = string.Empty, // TODO: Find mapping
                OrganizationId = organizationId.GetValueOrDefault()
            };
        }
    }
}