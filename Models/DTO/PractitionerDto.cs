﻿using System;
using CRISP.GRPC.ClinicalRelationship;
using Google.Protobuf.Collections;

namespace ProtoApp.Models.DTO
{
    public class PractitionerDto
    {
        public long Id { get; set; }
        public DemographicsDto Demographics { get; set; }
        public string Type { get; set; }
        public long OrganizationId { get; set; }
        public NameDto Name { get; set; }

        public string MedicalSpeciality { get; set; }
        public LicenseInformationDto LicenseInformation { get; set; }

        public Practitioner ToGrpc()
        {
            return new Practitioner();
        }
    }
}