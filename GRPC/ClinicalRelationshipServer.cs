using System.Reflection.Metadata;
using System.Threading.Tasks;
using CRISP.GRPC.ClinicalRelationship;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ProtoApp.GRPC
{
    public class ClinicalRelationshipServer : ClinicalRelationshipService.ClinicalRelationshipServiceBase
    {
        private readonly ILogger<ClinicalRelationshipServer> _logger;
        private readonly IClinicalRelationshipDelegate _serviceDelegate;

        public ClinicalRelationshipServer(ILogger<ClinicalRelationshipServer> logger, IClinicalRelationshipDelegate serviceDelegate)
        {
            _logger = logger;
            _serviceDelegate = serviceDelegate;
        }

        public override async Task<ClinicalRelationshipResponse> GetClinicalRelationship(ClinicalRelationshipRequest request, ServerCallContext context)
        {
            _logger.LogInformation(":: Performing {Class}.{Method}", nameof(IClinicalRelationshipDelegate), nameof(IClinicalRelationshipDelegate.Handle));
            return await _serviceDelegate.Handle(request);
        }
    }
}