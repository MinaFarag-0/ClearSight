﻿namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class DoctorProfileDto
    {
        public string DoctorId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string? AvailableFrom { get; set; }
        public string? AvailableTo { get; set; }
        public List<string>? DaysOff { get; set; } = [];
        public string[]? PhoneNumbers { get; set; }
        public string ProfileImagePath { get; set; }
        public bool AvailableForCureentMonth { get; set; }
        public string? Address { get; set; }
    }
}
