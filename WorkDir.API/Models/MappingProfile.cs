using AutoMapper;
using WorkDir.API.Models.DataModels;
using WorkDir.Domain.Entities;

namespace WorkDir.API.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserInfoDto>();

        CreateMap<User, UserRegisterResponseDto>().ForMember(r => r.id, c => c.MapFrom(u => u.UserId));

        CreateMap<User, UserInfoDto>()
            .ForMember(r => r.id, c => c.MapFrom(u => u.UserId))
            .ForMember(r => r.created_at, c => c.MapFrom(u => u.CreationDate));

        CreateMap<User, UserListInfoDto>()
            .ForMember(r => r.id, c => c.MapFrom(u => u.UserId))
            .ForMember(r => r.email, c => c.MapFrom(u => u.Email));

        CreateMap<Item, ItemDetailsDto>()
            .ForMember(r => r.id, c => c.MapFrom(i => i.Id))
            .ForMember(r => r.name, c => c.MapFrom(i => i.FileName));

        CreateMap<Item, ParentFolderInfoDto>()
            .ForMember(r => r.name, c => c.MapFrom(i => i.FileName))
            .ForMember(r => r.id, c => c.MapFrom(i => i.Id))
            .ForMember(r => r.owner_id, c => c.MapFrom(i => i.OwnerId));

        CreateMap<FolderDto, HomeFolderDto>();

    }
}
