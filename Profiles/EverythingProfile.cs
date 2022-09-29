using AutoMapper;
using MusicStreamingService.Models;

namespace MusicStreamingService.Profiles
{
    public class EverythingProfile : Profile
    {
        public EverythingProfile()
        {
            // source => target
            CreateMap<DbUserSong, Song>()
                .ForMember(e => e.FileName, opt => opt.MapFrom(e => e.DbSong.FileName))
                .ForMember(e => e.Duration, opt => opt.MapFrom(e => e.DbSong.Duration))
                .ForMember(e => e.BitRate, opt => opt.MapFrom(e => e.DbSong.BitRate))
                .ForMember(e => e.UserIndex, opt => opt.MapFrom(e => e.UserIndex))
                .ForMember(e => e.Path, opt => opt.MapFrom(e => e.DbSong.FileName))
                .ForMember(e => e.CreatedUtcDateTime, opt => opt.MapFrom(e => e.DbSong.CreatedUtcDateTime));

            CreateMap<DbSong, DbUserSong>()
                .ForMember(e => e.DbSong, opt => opt.MapFrom(e => e))
                .ForMember(e => e.DbSongId, opt => opt.MapFrom(e => e.Id));
        }
    }
}
