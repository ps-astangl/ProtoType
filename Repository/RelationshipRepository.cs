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
        public Task<List<Relationship>> GetRelationshipsByEid(string eid, string datasource);
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

        public Task<List<Relationship>> GetRelationshipsByEid(string eid, string datasource = "Panel")
        {
            try
            {
                var query =
                    _context
                        .Relationships
                        .Where(x => x.Patient.Eid.Equals(eid) && x.DataSource.Equals(datasource))
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