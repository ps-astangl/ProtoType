﻿
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Context.Context;
using CRISP.GRPC.ClinicalRelationship;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProtoApp.Models.DTO;
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

        // TODO: Move into repository layer for queries.
        // TODO: Move map and merge operations behind interface
        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger, PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            var eid = clinicalRelationshipRequest.PatientIdentifiers.Eid;

            // FROM DTO: Automap to GRPC response.
            var relationshipQuery =
                from patient in _context.Patients
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization.RelationshipId
                // join program in _context.OrganizationPrograms on organization.Id equals program.OrganizationId
                where patient.Eid == eid
                select
                    new RelationshipDto
                    {
                        Id = relationship.Id,
                        DataSource = relationship.DataSource,
                        Source = patient.Source,
                        Mrn = patient.Mrn,
                        Organization = new OrganizationDto
                        {
                            Id = organization.Id,
                            RelationshipId = relationship.Id,
                            DataSource = relationship.DataSource,
                            Name = organization.ParticipantName,
                            Source = organization.AssigningAuthorityCode,
                            SubstanceUseDisclosure = organization.SubstancesUseDisclosure,
                            Demographics = new DemographicsDto
                            {
                                Id = organization.Demographic.Id,
                                PhoneNumber = organization.Demographic.PhoneNumber,
                                Email = organization.Demographic.Email,
                                City = organization.Demographic.City,
                                State = organization.Demographic.State,
                                Zip = organization.Demographic.Zip,
                                AddressLine1 = organization.Demographic.AddressLine1,
                                AddressLine2 = organization.Demographic.AddressLine2
                            }
                        },
                        // Program = new ProgramDto
                        // {
                        //     Id = program.Id,
                        //     Name = program.Name,
                        //     Description = program.Description,
                        //     OrganizationId = program.OrganizationId
                        // }
                    };

            /*
            // For Now restrict this to 2 independent queries.
            var practitionerQuery =
                from patient in _context.Patients
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join practitioner in _context.Practitioners on relationship.Id equals practitioner.RelationshipId
                where patient.Eid == eid
                select new PractitionerDto
                {
                    Id = practitioner.Id,
                    RelationshipId = relationship.Id,
                    Name = new NameDto
                    {
                        DisplayName = practitioner.DisplayName
                    },
                    Demographics = new DemographicsDto
                    {
                        PhoneNumber = practitioner.Demographic.PhoneNumber,
                        Email = practitioner.Demographic.Email,
                        City = practitioner.Demographic.City,
                        State = practitioner.Demographic.State,
                        Zip = practitioner.Demographic.Zip,
                        AddressLine1 = practitioner.Demographic.AddressLine1,
                        AddressLine2 = practitioner.Demographic.AddressLine2
                    },
                    MedicalSpeciality = practitioner.MedicalSpeciality,
                    OrganizationId = practitioner.OrganizationId,
                    LicenseInformation = new LicenseInformationDto
                    {
                        Type = practitioner.LicenseType,
                        Value = practitioner.License
                    }
                };
                */

            // All relationships
            var relationshipResult = await relationshipQuery.ToListAsync();

            // Do we need the data specific to the Source and MRN for the Relationship level information
            var mapped = MapToRelationship(relationshipResult);
            _logger.LogInformation(":: Relationships Found {NumberOfRelationships}", relationshipResult.Count);

            // Relationship result
            // var practitionerResult = await practitionerQuery.ToListAsync();

            // var practitioners = MapPractitioner(practitionerResult);
            // _logger.LogInformation(":: Organizations Found {NumberOfPractitioners}", practitionerResult.Count);

            var organizations = MapToOrganizations(relationshipResult);
            _logger.LogInformation(":: Organizations Found {NumberOfOrganizations}", organizations.Count);

            var programs = MapToPrograms(relationshipResult);
            _logger.LogInformation(":: Programs Found {NumberOfPrograms}", programs.Count);

            var mergedOrganizations = MergeOrganizationsAndPrograms(programs, organizations);

            return new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = { organizations },
                    Practitioners = { }
                },
                Error = null
            };
        }

        // Practitioner
        private static List<Practitioner> MapPractitioner(List<PractitionerDto> practitionerResult)
        {
            var practitioners = practitionerResult
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpc())
                ?.ToList();

            return practitioners;
        }

        // Primary Relationships
        private static List<PatientRelationship> MapToRelationship(List<RelationshipDto> relationship)
        {
            return relationship
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpc())
                .ToList();
        }

        // Organizations
        private static List<Organization> MapToOrganizations(List<RelationshipDto> relationshipResult)
        {
            var organizations = relationshipResult
                ?.Where(x => x?.Organization != null)
                ?.Select(x => x?.Organization?.ToGrpc())
                ?.ToList();
            return organizations;
        }

        // Programs
        private static List<CRISP.GRPC.ClinicalRelationship.Program> MapToPrograms(List<RelationshipDto> relationshipResult)
        {
            var programs = relationshipResult
                ?.Where(x => x?.Program != null)
                ?.Select(x => x?.Program?.ToGrpc())
                ?.ToList();
            return programs;
        }

        // Join Programs and Organizations
        private static List<Organization> MergeOrganizationsAndPrograms(List<CRISP.GRPC.ClinicalRelationship.Program> programs, List<Organization> organizations)
        {
            var mergedOrganizations =
                (from program in programs
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
                    }).ToList();
            return mergedOrganizations;
        }
    }
}