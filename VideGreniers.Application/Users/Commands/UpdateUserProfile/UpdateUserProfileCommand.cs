using ErrorOr;
using MediatR;

namespace VideGreniers.Application.Users.Commands.UpdateUserProfile;

/// <summary>
/// Command to update user profile information
/// </summary>
public record UpdateUserProfileCommand(
    string FirstName,
    string LastName,
    string? Phone = null,
    string? Bio = null,
    string? AvatarUrl = null,
    string? PreferredLanguage = null,
    bool? EmailNotifications = null,
    bool? PushNotifications = null,
    bool? SmsNotifications = null) : IRequest<ErrorOr<Success>>;