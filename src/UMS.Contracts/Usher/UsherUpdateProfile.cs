using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.Usher
{
    public sealed class UpdateUsherProfileRequest
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string? ExperienceSummary { get; set; }
        public List<Language> Languages { get; set; } = [];
        public List<Sector>? Sector { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }
}
