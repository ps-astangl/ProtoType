
using System.Linq;
using System.Threading.Tasks;
using Context.Context;
using CRISP.GRPC.ClinicalRelationship;
using Google.Protobuf.Collections;
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

        public ClinicalRelationshipDelegate(ILogger<ClinicalRelationshipDelegate> logger,
            PatientRelationshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ClinicalRelationshipResponse> Handle(ClinicalRelationshipRequest clinicalRelationshipRequest)
        {
            //TODO: Break query into two separate ones and join at end from DTO.
            // FROM DTO: Automap to GRPC response.
            var relationshipQuery = from patient in _context.Patients
                join relationship in _context.Relationships on patient.Id equals relationship.PatientId
                join organization in _context.Organizations on relationship.Id equals organization.RelationshipId
                join program in _context.OrganizationPrograms on organization.Id equals program.OrganizationId
                // join practitioner in _context.Practitioners on relationship.Id equals practitioner.RelationshipId
                where patient.Eid == "123456"
                select
                    new RelationshipDto
                    {
                        Id = relationship.Id,
                        Organization = new OrganizationDto
                        {
                            Id = organization.Id,
                            DataSource = relationship.DataSource,
                            Name = organization.ParticipantName,
                            Source = organization.ParticipantSourceCode,
                            SubstanceUseDisclosure = organization.SubstancesUseDisclosure,
                            Demographics = new DemographicsDto
                            {
                                PhoneNumber = organization.Demographic.PhoneNumber,
                                Email = organization.Demographic.Email,
                                City = organization.Demographic.City,
                                State = organization.Demographic.State,
                                Zip = organization.Demographic.Zip,
                                AddressLine1 = organization.Demographic.AddressLine1,
                                AddressLine2 = organization.Demographic.AddressLine2
                            }
                        },
                        Program = new ProgramDto
                        {
                            Id = program.Id,
                            Name = program.Name,
                            Description = program.Description,
                            OrganizationId = program.OrganizationId
                        }
                    };

            var practitionerQuery = from practitioner in _context.Practitioners
                join relationship in _context.Relationships on practitioner.RelationshipId equals relationship.Id
                select new PractitionerDto
                {
                    Id = practitioner.Id,
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
                    Type = practitioner.MedicalSpeciality,
                    OrganizationId = practitioner.OrganizationId.Value,
                    LicenseInformation = new LicenseInformationDto
                    {
                        Type = practitioner.LicenseType,
                        Value = practitioner.License
                    }
                };
            // All relationships
            var relationshipResult = await relationshipQuery.ToListAsync();
            // var practitionerResult = await practitionerQuery.ToListAsync();

            // Organizations
            var organizations = relationshipResult
                ?.Where(x => x?.Organization != null)
                ?.Select(x => x?.Organization?.ToGrpc())
                ?.ToList();

            var programs = relationshipResult
                ?.Where(x => x?.Program != null)
                ?.Select(x => x?.Program?.ToGrpc())
                ?.ToList();

            // Join Programs and Organizations
            var mergedOrganizations =
                from program in programs
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
                    };

            return new ClinicalRelationshipResponse
            {
                PatientRelationships = new PatientRelationship
                {
                    Organizations = {mergedOrganizations},
                    Practitioners = {}
                },
                Error = null
            };
        }
    }
}