using AutoMapper;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Models;
using ClearSight.Core.Mosels;

namespace ClearSight.Core.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Doctor, DoctorProfileDto>()
                .ForMember(m => m.FullName, s => s.MapFrom(s => s.User.FullName))
                .ForMember(m => m.UserName, s => s.MapFrom(s => s.User.UserName))
                .ForMember(m => m.ProfileImagePath, s => s.MapFrom(s => s.User.ProfileImagePath))
                .ForMember(m => m.PhoneNumbers, s => s.MapFrom(s => s.User.PhoneNumbers.Select(p => p.PhoneNumber).ToList()))
                .ForMember(m => m.DaysOff, s => s.MapFrom(s => DaysOffHelper.ConvertToStringList(s.DaysOff)))
                .ForMember(m => m.AvailableFrom, s => s.MapFrom(s => s.AvailableFrom.ToShortTimeString()))
                .ForMember(m => m.AvailableTo, s => s.MapFrom(s => s.AvailableTo.ToShortTimeString()));
            CreateMap<Doctor, DoctorActivateProfile>()
                .ForMember(m => m.FullName, s => s.MapFrom(s => s.User.FullName))
                .ForMember(m => m.UserName, s => s.MapFrom(s => s.User.UserName))
                .ForMember(m => m.ProfileImagePath, s => s.MapFrom(s => s.User.ProfileImagePath))
                .ForMember(m => m.PhoneNumbers, s => s.MapFrom(s => s.User.PhoneNumbers.Select(p => p.PhoneNumber).ToList()));
            CreateMap<Patient, PatientProfileDto>()
                .ForMember(m => m.PatientId, s => s.MapFrom(s => s.User.Id))
                .ForMember(m => m.UserName, s => s.MapFrom(s => s.User.UserName))
                .ForMember(m => m.FullName, s => s.MapFrom(s => s.User.FullName))
                .ForMember(m => m.ProfileImagePath, s => s.MapFrom(s => s.User.ProfileImagePath))
                .ForMember(m => m.PhoneNumbers, s => s.MapFrom(s => s.User.PhoneNumbers.Select(p => p.PhoneNumber).ToList()))
                .ForMember(m => m.Email, s => s.MapFrom(s => s.User.Email));
            CreateMap<PatientHistory, PatientHistoryDto>()
                .ForMember(m => m.Date, s => s.MapFrom(s => s.Date.ToLocalTime()))
                .ForMember(m => m.DoctorName, s => s.MapFrom(s => s.Doctor.User.FullName));
        }
    }
}
