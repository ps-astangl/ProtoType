using System.Threading.Tasks;
using CRISP.GRPC.ClinicalRelationship;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProtoApp.Handler;

namespace ProtoApp.Server
{
    public class ClinicalRelationshipServer : ClinicalRelationshipService.ClinicalRelationshipServiceBase
    {
        private readonly ILogger<ClinicalRelationshipServer> _logger;
        private readonly IClinicalRelationshipHandler _serviceHandler;

        public ClinicalRelationshipServer(ILogger<ClinicalRelationshipServer> logger, IClinicalRelationshipHandler serviceHandler)
        {
            _logger = logger;
            _serviceHandler = serviceHandler;
        }

        public override async Task<ClinicalRelationshipResponse> GetClinicalRelationship(ClinicalRelationshipRequest request, ServerCallContext context)
        {
            _logger.LogInformation(":: Performing {Class}.{Method}", nameof(IClinicalRelationshipHandler), nameof(IClinicalRelationshipHandler.Handle));
            return await _serviceHandler.Handle(request);
        }
    }
}