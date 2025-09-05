using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Interfaces;

/// <summary>
/// Repository interface for UserWithAuth entity
/// </summary>
public interface IUserWithAuthRepository
{
    Task<UserWithAuth?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserWithAuth?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserWithAuth?> GetByExternalAuthAsync(AuthProvider provider, string externalId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    void Add(UserWithAuth user);
    void Update(UserWithAuth user);
    void Delete(UserWithAuth user);
}