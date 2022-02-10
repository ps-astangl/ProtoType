using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Context.Context;
using Context.Context.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ProtoApp.Repository
{
    public interface IRelationshipRepository
    {
        public Task<List<Relationship>> GetRelationshipsByEid(string eid);
    }

    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly ILogger<RelationshipRepository> _logger;
        private readonly PatientRelationshipContext _context;

        public RelationshipRepository(ILogger<RelationshipRepository> logger, PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        public Task<List<Relationship>> GetRelationshipsByEid(string eid)
        {
            try
            {
                var query =
                    _context
                        .Relationships
                        .Where(x => x.Patient.Eid.Equals(eid))
                        .Include(r => r.Organizations).ThenInclude(x => x.OrganizationPrograms)
                        .Include(r => r.Practitioners).ThenInclude(x => x.Demographic)
                        .ToListAsync();

                return query;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception has occured while performing the query");
                throw;
            }
        }

        /* This is a fall back option that we don't really wanna do...
        public Task<List<RelationshipDto>> SearchForRelationshipsAndOrganizations(string eid)
        {
            var query =
                from patient in _context.Patients
                where patient.Eid == eid
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization
                    .RelationshipId
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
            return query.ToListAsync();
        }

        public Task<List<ProgramDto>> SearchForProgramsByRelation(List<long> organizationIds)
        {
            var query =
                from program in _context.OrganizationPrograms
                join organization in _context.Organizations on program.OrganizationId equals organization.Id
                where organizationIds.Contains(organization.Id)
                select new ProgramDto
                {
                    Id = program.Id,
                    Name = program.Name,
                    Description = program.Description,
                    OrganizationId = program.OrganizationId
                };
            return query.ToListAsync();
        }

        public Task<List<PractitionerDto>> SearchForPractitionerByRelation(List<long> relationshipId)
        {
            var query = from practitioner in _context.Practitioners
                where relationshipId.Contains(practitioner.RelationshipId ?? 0)
                select new PractitionerDto
                {
                    Id = practitioner.Id,
                    RelationshipId = practitioner.RelationshipId,
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
            return query.ToListAsync();
        }
    }
    */
    }
}