using System;
using System.Linq;
using System.Threading.Tasks;
using Context.Context;
using Context.Context.Models;
using CRISP.GRPC.ClinicalRelationship;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Address = CRISP.GRPC.ClinicalRelationship.Address;
using Organization = CRISP.GRPC.ClinicalRelationship.Organization;
using Practitioner = CRISP.GRPC.ClinicalRelationship.Practitioner;

namespace ProtoApp.GRPC
{
    public interface IClinicalRelationshipDelegate
    {
        public Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest);
    }

    public class ClinicalRelationshipDelegate : IClinicalRelationshipDelegate
    {
        private readonly ILogger<ClinicalRelationshipDelegate> _logger;
        private readonly PatientRelationshipContext _context;

        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger, PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            var relationshipQuery = from patient in _context.Patients
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization.RelationshipId
                join program in _context.OrganizationPrograms on organization.Id equals program.OrganizationId
                join practitioner in _context.Practitioners on relationship.Id equals practitioner.RelationshipId
                where relationship.Id.Equals(3)
                select
                    new RelationshipDTO
                    {
                        Id = relationship.Id,
                        Organization = new OrganizationDTO
                        {
                            Id = organization.Id,
                            DataSource = relationship.DataSource,
                            Name = organization.ParticipantName,
                            Source = organization.ParticipantSourceCode,
                            SubstanceUseDisclosure = organization.SubstancesUseDisclosure,
                            ContactInformation = new ContactInformationDTO
                            {
                                Phone = new PhoneNumberDTO
                                {
                                    Number = organization.Demographic.PhoneNumber
                                },
                                Email = organization.Demographic.Email
                            },
                            Address = new AddressDTO
                            {
                                City = organization.Demographic.City,
                                State = organization.Demographic.State,
                                Zip = organization.Demographic.Zip,
                                AddressLine1 = organization.Demographic.AddressLine1,
                                AddressLine2 = organization.Demographic.AddressLine2
                            }
                        },
                        Program = new ProgramDTO
                        {
                            Id = program.Id,
                            Name = program.Name,
                            Description = program.Description,
                            OrganizationId = program.OrganizationId
                        },
                        Practitioner = new PractitionerDTO
                        {
                            Id = practitioner.Id,
                            Name = new NameDTO
                            {
                                Firstname = string.Empty, LastName = string.Empty, MiddleName = string.Empty,
                                DisplayName = practitioner.DisplayName
                            },
                            Address = new AddressDTO
                            {
                                City = practitioner.Demographic.City,
                                State = practitioner.Demographic.State,
                                Zip = practitioner.Demographic.Zip,
                                AddressLine1 = practitioner.Demographic.AddressLine1,
                                AddressLine2 = practitioner.Demographic.AddressLine2
                            },
                            ContactInformation = new ContactInformationDTO
                            {
                                Phone = new PhoneNumberDTO
                                {
                                    Type = string.Empty, Number = practitioner.Demographic.PhoneNumber
                                },
                                Email = practitioner.Demographic.Email
                            },
                            Type = practitioner.Type,
                            OrganizationId = practitioner.OrganizationId
                        }
                    };


            // All relationships
            var relationships = await relationshipQuery.ToListAsync();

            // Organizations
            var organizations = relationships?.Select(x => x.Organization.ToGrpc() ?? new Organization())?.ToList();

            // Practitioners
            var practitioners = relationships?.Select(x => x.Practitioner.ToGrpc() ?? new Practitioner())?.ToList();

            // Programs
            var programs = relationships?.Select(x => x?.Program?.ToGrpc() ?? new CRISP.GRPC.ClinicalRelationship.Program())?.ToList();

            // Merge the organizations
            var mergedOrganizations =
                from program in programs
                join organization in organizations on program?.OrganizationId equals organization?.Id
                select
                    new Organization
                    {
                        Id = organization.Id,
                        DataSource = organization.DataSource,
                        Name = organization.Name,
                        Source = organization.Source,
                        SubstanceUseDisclosure = organization.SubstanceUseDisclosure,
                        ContactInformation = organization.ContactInformation,
                        Address = organization.Address,
                        Programs = { program }
                    };
            return new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = { mergedOrganizations },
                    Practitioners = { practitioners }
                },
                Error = null
            };
        }
    }

    #region DataTransferObjects
    public class NameDTO
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string DisplayName { get; set; }

        public Name ToGrpc()
        {
            return new Name
            {
                Firstname = Firstname,
                MiddleName = MiddleName,
                LastName = LastName,
                DisplayName = DisplayName
            };
        }
    }
    public class AddressDTO
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public Address ToGrpc()
        {
            return new Address
            {
                City = City,
                State = State,
                Zip = Zip,
                AddressLine1 = AddressLine1,
                AddressLine2 = AddressLine2 ?? string.Empty
            };
        }
    }
    public class PhoneNumberDTO
    {
        public string Type { get; set; }
        public string Number { get; set; }

        public PhoneNumber ToGrpc()
        {
            return new PhoneNumber
            {
                Number = Number,
                Type = PhoneNumber.Types.PhoneType.Work // Need to map from Type
            };
        }
    }
    public class RelationshipDTO
    {
        public OrganizationDTO Organization { get; set; }
        public PractitionerDTO Practitioner { get; set; }
        public ProgramDTO Program { get; set; }
        public long Id { get; set; }

        public Relationship ToGrpc()
        {
            return new Relationship
            {
                Id = Id,
                PatientId = null,
                DataSource = null,
                DateAdded = default,
                DateUpdated = default,
                Patient = null,
                Organizations = null,
                Practitioners = null
            };
        }
    }
    public class ProgramDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? OrganizationId { get; set; }
        public CRISP.GRPC.ClinicalRelationship.Program ToGrpc()
        {
            return new CRISP.GRPC.ClinicalRelationship.Program
            {
                Id = Id,
                Name = Name,
                Description = Description,
                OrganizationId = OrganizationId ?? 0
            };
        }
    }
    public class PractitionerDTO
    {
        public long Id { get; set; }
        public ContactInformationDTO ContactInformation { get; set; }
        public string Type { get; set; }
        public long? OrganizationId { get; set; }
        public AddressDTO Address { get; set; }
        public NameDTO Name { get; set; }
        public Practitioner ToGrpc()
        {
            return new Practitioner
            {
                Id = Id,
                Name = Name.ToGrpc(),
                Address = Address.ToGrpc(),
                ContactInformation = ContactInformation.ToGrpc(),
                Type = Enum.TryParse<Practitioner.Types.ProviderType>(Type, out var type) ? type : Practitioner.Types.ProviderType.None
            };
        }
    }
    public class OrganizationDTO
    {
        public long Id { get; set; }
        public string DataSource { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public bool? SubstanceUseDisclosure { get; set; }
        public ContactInformationDTO ContactInformation { get; set; }
        public AddressDTO Address { get; set; }
        public Organization ToGrpc()
        {
            return new Organization
            {
                Id = Id,
                DataSource = DataSource,
                Name = Name,
                Source = Source,
                SubstanceUseDisclosure = SubstanceUseDisclosure ?? false,
                ContactInformation = ContactInformation.ToGrpc(),
                Address = Address.ToGrpc()
            };
        }
    }
    public class ContactInformationDTO
    {
        public PhoneNumberDTO Phone { get; set; }
        public string Email { get; set; }

        public ContactInformation ToGrpc()
        {
            return new ContactInformation
            {
                Phone = Phone.ToGrpc(),
                Email = Email
            };
        }
    }
    #endregion
}