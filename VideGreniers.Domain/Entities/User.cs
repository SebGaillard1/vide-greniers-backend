using ErrorOr;
using VideGreniers.Domain.Common.Errors;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.Events;
using VideGreniers.Domain.ValueObjects;

namespace VideGreniers.Domain.Entities;

public sealed class User : BaseAuditableEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime? LastLoginUtc { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly List<Favorite> _favorites = [];
    public IReadOnlyList<Favorite> Favorites => _favorites.AsReadOnly();

    private readonly List<Event> _createdEvents = [];
    public IReadOnlyList<Event> CreatedEvents => _createdEvents.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    private User(
        string firstName,
        string lastName,
        Email email,
        UserRole role = UserRole.User,
        PhoneNumber? phoneNumber = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
        PhoneNumber = phoneNumber;
        IsEmailVerified = false;
        IsActive = true;

        RaiseDomainEvent(new UserRegisteredDomainEvent(Id, Email.Value));
    }

    public static ErrorOr<User> Create(
        string firstName,
        string lastName,
        string email,
        UserRole role = UserRole.User,
        string? phoneNumber = null)
    {
        var errors = new List<Error>();

        // Validate first name
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length < 2)
        {
            errors.Add(Errors.User.NameTooShort);
        }

        if (firstName?.Length > 100)
        {
            errors.Add(Errors.User.NameTooLong);
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length < 2)
        {
            errors.Add(Errors.User.NameTooShort);
        }

        if (lastName?.Length > 100)
        {
            errors.Add(Errors.User.NameTooLong);
        }

        // Validate email
        var emailResult = Email.Create(email);
        if (emailResult.IsError)
        {
            errors.AddRange(emailResult.Errors);
        }

        // Validate phone number if provided
        PhoneNumber? phoneNumberValue = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phoneResult = PhoneNumber.Create(phoneNumber);
            if (phoneResult.IsError)
            {
                errors.AddRange(phoneResult.Errors);
            }
            else
            {
                phoneNumberValue = phoneResult.Value;
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new User(
            firstName!.Trim(),
            lastName!.Trim(),
            emailResult.Value,
            role,
            phoneNumberValue);
    }

    public ErrorOr<Updated> UpdateProfile(
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        var errors = new List<Error>();

        // Validate first name
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length < 2)
        {
            errors.Add(Errors.User.NameTooShort);
        }

        if (firstName?.Length > 100)
        {
            errors.Add(Errors.User.NameTooLong);
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length < 2)
        {
            errors.Add(Errors.User.NameTooShort);
        }

        if (lastName?.Length > 100)
        {
            errors.Add(Errors.User.NameTooLong);
        }

        // Validate phone number if provided
        PhoneNumber? phoneNumberValue = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phoneResult = PhoneNumber.Create(phoneNumber);
            if (phoneResult.IsError)
            {
                errors.AddRange(phoneResult.Errors);
            }
            else
            {
                phoneNumberValue = phoneResult.Value;
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        FirstName = firstName!.Trim();
        LastName = lastName!.Trim();
        PhoneNumber = phoneNumberValue;
        MarkAsModified();

        RaiseDomainEvent(new UserProfileUpdatedDomainEvent(Id));

        return Result.Updated;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
        {
            return;
        }

        IsEmailVerified = true;
        MarkAsModified();

        RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email.Value));
    }

    public void UpdateLastLogin()
    {
        LastLoginUtc = DateTime.UtcNow;
        MarkAsModified();
    }

    public void ChangeRole(UserRole newRole)
    {
        if (Role == newRole)
        {
            return;
        }

        var oldRole = Role;
        Role = newRole;
        MarkAsModified();

        RaiseDomainEvent(new UserRoleChangedDomainEvent(Id, oldRole, newRole));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsModified();

        RaiseDomainEvent(new UserDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkAsModified();

        RaiseDomainEvent(new UserActivatedDomainEvent(Id));
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public string GetInitials() => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpperInvariant();

    public bool CanCreateEvents() => Role is UserRole.Organizer or UserRole.Admin or UserRole.Moderator;

    public bool CanModerateContent() => Role is UserRole.Admin or UserRole.Moderator;

    public bool IsSystemAdmin() => Role == UserRole.Admin;

    // Methods for managing favorites (to be called from Favorite entity)
    internal void AddFavorite(Favorite favorite)
    {
        if (_favorites.Any(f => f.EventId == favorite.EventId && f.Status == FavoriteStatus.Active))
        {
            return; // Already favorited
        }

        _favorites.Add(favorite);
    }

    internal void RemoveFavorite(Favorite favorite)
    {
        _favorites.Remove(favorite);
    }

    // Methods for managing created events (to be called from Event entity)
    internal void AddCreatedEvent(Event @event)
    {
        if (!_createdEvents.Contains(@event))
        {
            _createdEvents.Add(@event);
        }
    }
}