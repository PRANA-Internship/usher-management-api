using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using UMS.Domain.Enums;
namespace UMS.Contracts.Usher
{

    public sealed class SubmitUsherApplicationRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;


        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
        public string ExperienceSummary { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;


        public IFormFile ProfilePhoto { get; set; } = null!;
        public IFormFile IdDocument { get; set; } = null!;
    }
}
