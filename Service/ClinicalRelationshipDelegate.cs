﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Context.Context.Models;
using CRISP.GRPC.ClinicalRelationship;
using Microsoft.Extensions.Logging;
using ProtoApp.Models.Mapping;
using ProtoApp.Repository;
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
        private readonly IRelationshipRepository _relationshipRepository;

        // TODO: Move into repository layer for queries.
        // TODO: Move map and merge operations behind interface
        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger, IRelationshipRepository relationshipRepository)
        {
            _logger = logger;
            _relationshipRepository = relationshipRepository;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            var relationships = await _relationshipRepository.GetRelationshipsByEid(clinicalRelationshipRequest.PatientIdentifiers.Eid);
            var response = InitializeResponse();
            if (relationships?.Count == 0 || relationships == null)
                return response;

            // Loop through each result and collect the information for the repeated response elements.
            foreach (var relationship in relationships)
            {
                var orgs = ExtractOrganizations(relationship.Organizations);
                var providers = ExtractProviders(relationship.Practitioners);

                if (orgs?.Count != 0)
                    response.PatientRelationships.Organizations.Add(orgs);

                if (providers?.Count != 0)
                    response.PatientRelationships.Practitioners.Add(providers);
            }

            _logger.LogInformation(":: Relationships Found {NumberOfRelationships}", relationships?.Count);
            _logger.LogInformation(":: Organizations Found {NumberOfOrganizations}", response?.PatientRelationships?.Organizations?.Count);
            _logger.LogInformation(":: Practitioners Found {NumberOfPractitioners}", response?.PatientRelationships?.Practitioners?.Count);

            return response;
        }
        private static List<Organization> ExtractOrganizations(IEnumerable<Context.Context.Models.Organization> input)
        {
            var organization = input
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcOrganization())
                ?.ToList();
            return organization;
        }
        private static List<Practitioner> ExtractProviders(IEnumerable<Context.Context.Models.Practitioner> input)
        {
            var providers = input
                ?.Where(x => x != null)
                ?.Select(x => x.ToGrpcPractitioner())
                ?.ToList();
            return providers;
        }


        private static ClinicalRelationshipResponse InitializeResponse()
        {
            var response = new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = { },
                    Practitioners = { }
                },
                Error = null
            };
            return response;
        }
    }
}