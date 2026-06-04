using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.DTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? TenderNumber { get; set; }
        public string? ProjectLeadId { get; set; }
        public string ContractorName { get; set; } = string.Empty;
        public string ConsultantName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ContractSum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";

        // Location
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusInMeters { get; set; }

        // Foreign keys
        public Guid WardId { get; set; }
        public Guid? ClusterId { get; set; }
    }
}
