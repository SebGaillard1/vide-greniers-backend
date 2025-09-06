using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Authentication.Models;
using VideGreniers.Application.Common.DTOs;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Interfaces;
using VideGreniers.Infrastructure.Identity;

namespace VideGreniers.Infrastructure.Services;

/// <summary>
/// Implementation of IAuthenticationService
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthenticationResult>> RegisterAsync(string email, string password, string? firstName, string? lastName)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Error.Conflict("User.DuplicateEmail", "A user with this email already exists.");
        }

        // Create new user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AuthProvider = "Email",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create user with password
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.Validation("User.CreateFailed", $"Failed to create user: {errors}");
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Create authenticated user
        var authenticatedUser = new AuthenticatedUser(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(authenticatedUser, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = _jwtTokenService.GetAccessTokenExpiration();
        var expiresIn = (int)(accessTokenExpiry - DateTime.UtcNow).TotalSeconds;

        // Create UserDto
        var userDto = await CreateUserDtoAsync(user);

        // Store refresh token
        await _refreshTokenRepository.AddAsync(
            refreshTokenValue,
            user.Id.ToString(),
            _jwtTokenService.GetRefreshTokenExpiration(),
            null);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthenticationResult(
            accessToken,
            refreshTokenValue,
            expiresIn,
            userDto);
    }

    public async Task<ErrorOr<AuthenticationResult>> LoginAsync(string email, string password)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        // Check if user is active
        if (user.IsDeleted)
        {
            return Error.Unauthorized("Auth.AccountDeleted", "This account has been deleted.");
        }

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Error.Unauthorized("Auth.AccountLockedOut", "Account is locked out due to multiple failed login attempts.");
            }

            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        // Update last login time
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Create authenticated user
        var authenticatedUser = new AuthenticatedUser(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(authenticatedUser, roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = _jwtTokenService.GetAccessTokenExpiration();
        var expiresIn = (int)(accessTokenExpiry - DateTime.UtcNow).TotalSeconds;

        // Create UserDto
        var userDto = await CreateUserDtoAsync(user);

        // Store refresh token
        await _refreshTokenRepository.AddAsync(
            refreshTokenValue,
            user.Id.ToString(),
            _jwtTokenService.GetRefreshTokenExpiration(),
            null);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthenticationResult(
            accessToken,
            refreshTokenValue,
            expiresIn,
            userDto);
    }

    public async Task<ErrorOr<AuthenticationResult>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // Find the refresh token
        var tokenInfo = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (tokenInfo == null || tokenInfo.IsRevoked || tokenInfo.ExpiresAt <= DateTime.UtcNow)
        {
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid refresh token.");
        }

        // Get the user
        var user = await _userManager.FindByIdAsync(tokenInfo.UserId);
        if (user == null || user.IsDeleted)
        {
            return Error.Unauthorized("Auth.UserNotFound", "User not found.");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Create authenticated user
        var authenticatedUser = new AuthenticatedUser(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt);

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(authenticatedUser, roles);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiry = _jwtTokenService.GetAccessTokenExpiration();
        var expiresIn = (int)(accessTokenExpiry - DateTime.UtcNow).TotalSeconds;

        // Create UserDto
        var userDto = await CreateUserDtoAsync(user);

        // Revoke old token and create new one
        await _refreshTokenRepository.RevokeAsync(refreshToken);
        await _refreshTokenRepository.AddAsync(
            newRefreshTokenValue,
            user.Id.ToString(),
            _jwtTokenService.GetRefreshTokenExpiration(),
            null);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthenticationResult(
            newAccessToken,
            newRefreshTokenValue,
            expiresIn,
            userDto);
    }

    public async Task<ErrorOr<bool>> LogoutAsync(string refreshToken)
    {
        var tokenInfo = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (tokenInfo == null)
        {
            return Error.NotFound("Auth.RefreshTokenNotFound", "Refresh token not found.");
        }

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return true;
    }

    public async Task<ErrorOr<AuthenticatedUser>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        return new AuthenticatedUser(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt);
    }

    public async Task<ErrorOr<AuthenticatedUser>> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        return new AuthenticatedUser(
            user.Id.ToString(),
            user.Email!,
            user.FirstName,
            user.LastName,
            user.CreatedAt);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null && !user.IsDeleted;
    }

    private async Task<UserDto> CreateUserDtoAsync(ApplicationUser applicationUser)
    {
        // Try to get the domain user
        var domainUser = await _userRepository.GetByIdAsync(applicationUser.Id);
        
        if (domainUser != null)
        {
            return new UserDto
            {
                Id = domainUser.Id,
                FirstName = domainUser.FirstName,
                LastName = domainUser.LastName,
                Email = domainUser.Email.Value,
                PhoneNumber = domainUser.PhoneNumber?.Value,
                CreatedOnUtc = domainUser.CreatedOnUtc,
                ModifiedOnUtc = domainUser.ModifiedOnUtc,
                CreatedEventsCount = domainUser.CreatedEvents.Count,
                FavoritesCount = domainUser.Favorites.Count(f => f.Status == Domain.Enums.FavoriteStatus.Active)
            };
        }

        // Fallback to ApplicationUser data if domain user not found
        return new UserDto
        {
            Id = applicationUser.Id,
            FirstName = applicationUser.FirstName ?? string.Empty,
            LastName = applicationUser.LastName ?? string.Empty,
            Email = applicationUser.Email ?? string.Empty,
            PhoneNumber = null,
            CreatedOnUtc = applicationUser.CreatedAt,
            ModifiedOnUtc = applicationUser.UpdatedAt,
            CreatedEventsCount = 0,
            FavoritesCount = 0
        };
    }
}