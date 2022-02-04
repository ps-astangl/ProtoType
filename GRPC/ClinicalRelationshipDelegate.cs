using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRISP.GRPC.ClinicalRelationship;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using ProtoApp.Context;
using ProtoApp.Context.Models;
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

        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger,
            PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            /*TODO: Query for Organization based on patient
             * select * from Patient p
                inner join Relationship r on p.Id = r.PatientId inner join Organization org on r.Id = org.RelationshipId
                left join Address a on org.AddressId = a.Id
                left join Contact C on C.Id = org.ContactId
                left join OrganizationProgram OP on org.Id = OP.OrganizationId
                where p.Id = 1

                TODO: Figure out query for Practitioner (should be almost identical)
                TODO: Map result
             */
            var organizationQuery = from patient in _context.Patients
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization.RelationshipId
                join program in _context.OrganizationPrograms on organization.Id equals program.OrganizationId
                join practitioner in _context.Practitioners on relationship.Id equals practitioner.RelationshipId
                where patient.Eid.Equals(clinicalRelationshipRequest.PatientIdentifiers.Eid)
                select
                    new RelationshipDTO
                    {
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
                                    Number = organization.Contact.PhoneNumber
                                },
                                Email = organization.Contact.Email
                            },
                            Address = new AddressDTO
                            {
                                City = organization.Address.City,
                                State = organization.Address.State,
                                Zip = organization.Address.Zip,
                                AddressLine1 = organization.Address.AddressLine1,
                                AddressLine2 = organization.Address.AddressLine2
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
                                Firstname = String.Empty, LastName = String.Empty, MiddleName = String.Empty,
                                DisplayName = practitioner.DisplayName
                            },
                            Address = new AddressDTO
                            {
                                City = practitioner.Address.City,
                                State = practitioner.Address.State,
                                Zip = practitioner.Address.Zip,
                                AddressLine1 = practitioner.Address.AddressLine1,
                                AddressLine2 = practitioner.Address.AddressLine2
                            },
                            ContactInformation = new ContactInformationDTO
                            {
                                Phone = new PhoneNumberDTO
                                {
                                    Type = string.Empty, Number = practitioner.Contact.PhoneNumber
                                },
                                Email = practitioner.Contact.Email
                            },
                            Type = practitioner.Type,
                            OrganizationId = practitioner.OrganizationId
                        }
                    };


            var foo = await organizationQuery.ToListAsync();

            return new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = { },
                    Practitioners = { }
                },
                Error = null
            };
        }
    }

    public class NameDTO
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string DisplayName { get; set; }

        public Name ToGrpcName()
        {
            return new Name
            {
                Firstname = null,
                MiddleName = null,
                LastName = null,
                DisplayName = null
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
    }
    public class PhoneNumberDTO
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }
    public class RelationshipDTO
    {
        public OrganizationDTO Organization { get; set; }
        public PractitionerDTO Practitioner { get; set; }
        public ProgramDTO Program { get; set; }
    }
    public class ProgramDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? OrganizationId { get; set; }
    }
    public class PractitionerDTO
    {
        public long Id { get; set; }
        public ContactInformationDTO ContactInformation { get; set; }
        public string Type { get; set; }
        public long? OrganizationId { get; set; }
        public AddressDTO Address { get; set; }
        public NameDTO Name { get; set; }

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
    }
    public class ContactInformationDTO
    {
        public PhoneNumberDTO Phone { get; set; }
        public string Email { get; set; }
    }
}