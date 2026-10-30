using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class TimesheetsMapper : Profile
    {
        public TimesheetsMapper()
        {
            CreateMap<Timesheet, TimesheetDetailsViewModel>()
                .ForMember(d => d.DateStart, opt => opt.MapFrom(s => new DateTime(s.DateTimeStart.Year, s.DateTimeStart.Month, s.DateTimeStart.Day, 0, 0, 0)))
                .ForMember(d => d.TimeStart, opt => opt.MapFrom(s => new DateTime(s.DateTimeStart.Year, s.DateTimeStart.Month, s.DateTimeStart.Day, s.DateTimeStart.Hour, s.DateTimeStart.Minute, 0)))
                .ForMember(d => d.DateEnd, opt => opt.MapFrom(s => (s.DateTimeEnd != null) ? new DateTime(s.DateTimeEnd.Value.Year, s.DateTimeEnd.Value.Month, s.DateTimeEnd.Value.Day, 0, 0, 0) : (DateTime?)null))
                .ForMember(d => d.TimeEnd, opt => opt.MapFrom(s => (s.DateTimeEnd != null) ? new DateTime(s.DateTimeEnd.Value.Year, s.DateTimeEnd.Value.Month, s.DateTimeEnd.Value.Day, s.DateTimeEnd.Value.Hour, s.DateTimeEnd.Value.Minute, 0) : (DateTime?)null))
                .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name));

            CreateMap<Timesheet, TimesheetFormViewModel>()
                .ForMember(d => d.DateStart, opt => opt.MapFrom(s => new DateTime(s.DateTimeStart.Year, s.DateTimeStart.Month, s.DateTimeStart.Day, 0, 0, 0)))
                .ForMember(d => d.TimeStart, opt => opt.MapFrom(s => new DateTime(s.DateTimeStart.Year, s.DateTimeStart.Month, s.DateTimeStart.Day, s.DateTimeStart.Hour, s.DateTimeStart.Minute, 0)));

            CreateMap<TimesheetFormViewModel, Timesheet>()
                .ForMember(d => d.DateTimeStart, opt => opt.MapFrom(s => new DateTime(s.DateStart.Year, s.DateStart.Month, s.DateStart.Day, s.TimeStart.Hour, s.TimeStart.Minute, 0)))
                .ForMember(d => d.DateTimeEnd, opt => opt.MapFrom(s => (s.DateEnd != null && s.TimeEnd != null) ? new DateTime(s.DateEnd.Value.Year, s.DateEnd.Value.Month, s.DateEnd.Value.Day, s.TimeEnd.Value.Hour, s.TimeEnd.Value.Minute, 0) : (DateTime?)null));
        }
    }
}
