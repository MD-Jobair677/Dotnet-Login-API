using AutoMapper;
using BulkMail.Application.DTOs;
using BulkMail.Domain.User.Entities;

namespace BulkMail.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.UserRoles
                        .Select(ur => ur.Role.Name)
                        .ToList()))
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.UserRoles
                        .SelectMany(ur => ur.Role.RolePermissions)
                        .Select(rp => rp.Permission.Name)
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList()));

            CreateMap<User, UserAuthResponseDto>()
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserRoles,
                    opt => opt.MapFrom(src => src.UserRoles
                        .Select(ur => ur.Role.Name)
                        .ToList()))
                .ForMember(dest => dest.UserPermissions,
                    opt => opt.MapFrom(src => src.UserRoles
                        .SelectMany(ur => ur.Role.RolePermissions)
                        .Select(rp => rp.Permission.Name)
                        .Distinct()
                        .ToList()))
                .ForMember(dest => dest.Token, opt => opt.Ignore());

            CreateMap<Role, RoleListResponseDto>()
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.RolePermissions
                        .Select(rp => rp.Permission.Name)
                        .ToList()));

            CreateMap<Role, RoleDetailResponseDto>()
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.RolePermissions
                        .Select(rp => rp.Permission.Name)
                        .ToList()));

            CreateMap<Role, RoleResponseDto>()
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.RolePermissions
                        .Select(rp => rp.Permission.Name)
                        .ToList()));

            CreateMap<Permission, PermissionResponseDto>();

            CreateMap<Student, StudentResponseDto>();
            CreateMap<StudentProfile, StudentProfileDto>();
        }
    }
}
