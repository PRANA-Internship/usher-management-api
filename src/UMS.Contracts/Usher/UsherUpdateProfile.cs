using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Usher
{
    public sealed class UpdateUsherProfileRequest
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EducationLevel { get; set; }
        public string? ExperienceSummary { get; set; }
        public string? Languages { get; set; }
        public string? Sector { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }
}
