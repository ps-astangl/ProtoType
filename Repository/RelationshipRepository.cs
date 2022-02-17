using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crisp.ClinicalDataRepository.ClinicalRelationships;
using Crisp.ClinicalDataRepository.ClinicalRelationships.Context;
using Crisp.ClinicalDataRepository.ClinicalRelationships.Context.Models;
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
    }
}