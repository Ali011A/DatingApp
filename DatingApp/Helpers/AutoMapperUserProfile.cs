using AutoMapper;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Extensions;

namespace DatingApp.Helpers
{
    public class AutoMapperUserProfile : Profile
    {
        public AutoMapperUserProfile() 
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(d=> d.Age,o=>o.MapFrom(s=>s.DateOfBirth.CalculateAge()))
                .ForMember(d=>d.PhotoUrl,
                o=>o.MapFrom(s=>s.Photos.FirstOrDefault(x=>x.IsMain)!.Url));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto,AppUser>();
            CreateMap<RgisterDto,AppUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            CreateMap<string, DateTime>().ConvertUsing(s => DateTime.ParseExact(s, "dd/MM/yyyy", null));
               CreateMap<Message, MessageDto>()
                .ForMember(d => d.SenderPhotoUrl, o =>
                o.MapFrom(s => s.Sender.Photos.FirstOrDefault(x => x.IsMain)!.Url))
                .ForMember(d => d.RecipientPhotoUrl, o =>
                o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(x => x.IsMain)!.Url));
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => 
            d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}
