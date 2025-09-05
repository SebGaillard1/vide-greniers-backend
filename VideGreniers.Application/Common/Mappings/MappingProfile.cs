using System.Reflection;
using AutoMapper;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Domain.Entities;

namespace VideGreniers.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        
        CreateCustomMappings();
    }

    private void CreateCustomMappings()
    {
        // Event mappings - using basic mapping, complex mapping will be done manually in handlers
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.Location, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.StartDate, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.EndDate, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.EntryFeeAmount, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.EntryFeeCurrency, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.EarlyBirdFeeAmount, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.EarlyBirdFeeCurrency, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.OrganizerName, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.CategoryName, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.CategoryIcon, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.CategoryColor, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.DistanceKm, opt => opt.Ignore()) // Calculated separately
            .ForMember(dest => dest.IsFavorite, opt => opt.Ignore()) // Calculated separately  
            .ForMember(dest => dest.FavoriteCount, opt => opt.Ignore()); // Calculated separately

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber != null ? src.PhoneNumber.Value : null))
            .ForMember(dest => dest.CreatedEventsCount, opt => opt.Ignore()) // Calculated separately
            .ForMember(dest => dest.FavoritesCount, opt => opt.Ignore()); // Calculated separately

        // Favorite mappings
        CreateMap<Favorite, FavoriteDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : null));
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;

        var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();

        var argumentTypes = new Type[] { typeof(Profile) };

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethod(mappingMethodName);

            if (methodInfo != null)
            {
                methodInfo.Invoke(instance, new object[] { this });
            }
            else
            {
                var interfaces = type.GetInterfaces().Where(HasInterface).ToList();

                if (interfaces.Count > 0)
                {
                    foreach (var @interface in interfaces)
                    {
                        var interfaceMethodInfo = @interface.GetMethod(mappingMethodName, argumentTypes);

                        interfaceMethodInfo?.Invoke(instance, new object[] { this });
                    }
                }
            }
        }
    }
}