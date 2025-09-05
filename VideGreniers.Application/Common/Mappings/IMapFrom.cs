using AutoMapper;

namespace VideGreniers.Application.Common.Mappings;

/// <summary>
/// Interface for automatic AutoMapper profile registration
/// </summary>
/// <typeparam name="T">Source type to map from</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Configure custom mapping if needed
    /// </summary>
    /// <param name="profile">AutoMapper profile</param>
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}