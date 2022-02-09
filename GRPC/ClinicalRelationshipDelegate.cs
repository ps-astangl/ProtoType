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
        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger,
            PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            /* Ideal query for Obtaining Relationship-> Organization -> Program
             SELECT *
            FROM [Patient] AS [p]
            INNER JOIN [Relationship] AS [r] ON [p].[Id] = [r].[PatientId]
            LEFT JOIN [Organization] AS [o] ON [r].[Id] = [o].[RelationshipId]
            LEFT JOIN [OrganizationProgram] AS [o0] ON [o].[Id] = [o0].[OrganizationId]
            LEFT JOIN [Demographic] AS [d] ON [o].[DemographicId] = [d].[Id]
            WHERE [p].[Eid] = 157883854
             */
            var eid = clinicalRelationshipRequest.PatientIdentifiers.Eid;

            // FROM DTO: Automap to GRPC response.
            var relationshipQuery =
                from patient in _context.Patients
                where patient.Eid == eid
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization.RelationshipId
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
                        }
                    };

            // First get the primary relationship based on Patient -> relationship -> organization
            var relationshipResult = await relationshipQuery.ToListAsync();

            // Then match the programs to the relationship result
            var programQuery =
                from program in _context.OrganizationPrograms
                join organization in _context.Organizations on program.OrganizationId equals organization.Id
                where relationshipResult.Select(x => x.Organization.Id).Contains(organization.Id)
                select new ProgramDto
                {
                    Id = program.Id,
                    Name = program.Name,
                    Description = program.Description,
                    OrganizationId = program.OrganizationId
                };

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

            // Do we need the data specific to the Source and MRN for the Relationship level information
            _logger.LogInformation(":: Relationships Found {NumberOfRelationships}", relationshipResult.Count);


            var organizations = MapToOrganizations(relationshipResult);
            _logger.LogInformation(":: Organizations Found {NumberOfOrganizations}", organizations.Count);

            var programResult = await programQuery.ToListAsync();
            var programs = MapToPrograms(programResult);
            _logger.LogInformation(":: Programs Found {NumberOfPrograms}", programs.Count);

            var mergedOrganizations = InnerJoinPrograms(programs, organizations);

            var practitionerResult = await practitionerQuery.ToListAsync();
            var practitioners = MapPractitioner(practitionerResult);
            _logger.LogInformation(":: Programs Found {NumberOfpractitioners}", practitioners.Count);

            return new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = {mergedOrganizations},
                    Practitioners = {practitioners}
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

        // Practitioner
        private static List<CRISP.GRPC.ClinicalRelationship.Program> MapProgram(List<ProgramDto> programDtos)
        {
            var programs = programDtos
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpc())
                ?.ToList();

            return programs;
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
        private static List<CRISP.GRPC.ClinicalRelationship.Program> MapToPrograms(List<ProgramDto> programResult)
        {
            var programs = programResult
                ?.Where(x => x != null)
                ?.Select(x => x?.ToGrpc())
                ?.ToList();

            return programs;
        }

        // Join Programs and Organizations
        private static List<Organization> InnerJoinPrograms(List<CRISP.GRPC.ClinicalRelationship.Program> programs,
            List<Organization> organizations)
        {
            if (programs.Count == 0)
                return organizations;

            var query =
                from organization in organizations
                join program in programs on organization.Id equals program.OrganizationId
                    into gj
                select new Organization
                {
                    Id = organization.Id,
                    DataSource = organization.DataSource,
                    Name = organization.Name,
                    Source = organization.Source,
                    SubstanceUseDisclosure = organization.SubstanceUseDisclosure,
                    ContactInformation = organization.ContactInformation,
                    Address = organization.Address,
                    Programs = {gj}
                };
            return query.ToList();
        }
    }
}