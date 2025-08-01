﻿namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class DoctorActivateProfile
    {
        public string DoctorId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string? UploadedDocumentPath { get; set; }
        public string[]? PhoneNumbers { get; set; }
        public string ProfileImagePath { get; set; }
        public string? Address { get; set; }
    }
}
