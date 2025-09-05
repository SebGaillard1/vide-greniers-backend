using Microsoft.EntityFrameworkCore;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Interfaces;

namespace VideGreniers.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.DomainUsers
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }


    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DomainUsers
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.DomainUsers
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        _context.DomainUsers.Add(user);
    }

    public void Update(User user)
    {
        _context.DomainUsers.Update(user);
    }

    public void Delete(User user)
    {
        _context.DomainUsers.Remove(user);
    }
}