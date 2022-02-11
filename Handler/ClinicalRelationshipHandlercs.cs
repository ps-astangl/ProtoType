using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Context.Context.Models;
using CRISP.GRPC.ClinicalRelationship;
using Microsoft.Extensions.Logging;
using ProtoApp.Mapper;
using ProtoApp.Repository;

namespace ProtoApp.Handler
{
    public interface IClinicalRelationshipHandler
    {
        public Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest);
    }

    public class ClinicalRelationshipHandler : IClinicalRelationshipHandler
    {
        private readonly ILogger<ClinicalRelationshipHandler> _logger;
        private readonly IRelationshipRepository _relationshipRepository;

        public ClinicalRelationshipHandler(ILogger<ClinicalRelationshipHandler> logger, IRelationshipRepository relationshipRepository)
        {
            _logger = logger;
            _relationshipRepository = relationshipRepository;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            var response = InitializeResponse();
            List<Relationship> relationships = null;
            try
            {
                relationships = await _relationshipRepository.GetRelationshipsByEid(
                    clinicalRelationshipRequest.PatientIdentifiers.Eid);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, ":: An exception has occured during execution of query");
                response.Error = new Error
                {
                    ErrorReason = exception.Message
                };
            }
            if (relationships?.Count == 0 || relationships == null)
                return response;

            // Loop through each result and collect the information for the repeated response elements.
            // foreach (var relationship in relationships)
            // {
            //     relationship.ToGrpcPatientRelationship();
            //     var orgs =     ExtractOrganizations(relationship.Organizations);
            //     var providers = ExtractProviders(relationship.Practitioners);
            //
            //     if (orgs?.Count != 0)
            //         response.PatientRelationships.Organizations.Add(orgs);
            //
            //     if (providers?.Count != 0)
            //         response.PatientRelationships.Practitioners.Add(providers);
            // }

            var foo = ExtractRelationship(relationships);
            response.PatientRelationships.Add(foo);

            _logger.LogInformation(":: Relationships Found {NumberOfRelationships}", relationships?.Count);
            // _logger.LogInformation(":: Organizations Found {NumberOfOrganizations}", response?.PatientRelationships?.Organizations?.Count);
            // _logger.LogInformation(":: Practitioners Found {NumberOfPractitioners}", response?.PatientRelationships?.Practitioners?.Count);

            return response;
        }
        private static List<CRISP.GRPC.ClinicalRelationship.Organization> ExtractOrganizations(IEnumerable<Context.Context.Models.Organization> input)
        {
            var organization = input
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcOrganization())
                ?.ToList();
            return organization;
        }
        private static List<CRISP.GRPC.ClinicalRelationship.Practitioner> ExtractProviders(IEnumerable<Context.Context.Models.Practitioner> input)
        {
            var providers = input
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcPractitioner())
                ?.ToList();
            return providers;
        }


        private static ClinicalRelationshipResponse InitializeResponse()
        {
            ClinicalRelationshipResponse clinicalRelationshipResponse = new ClinicalRelationshipResponse
            {
                PatientRelationships = {  },
                Error = null
            };
            return clinicalRelationshipResponse;
        }

        private static List<PatientRelationship> ExtractRelationship(IEnumerable<Relationship> input)
        {
            var relations = input
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcPatientRelationship())
                ?.ToList();
            return relations;
        }
    }
}