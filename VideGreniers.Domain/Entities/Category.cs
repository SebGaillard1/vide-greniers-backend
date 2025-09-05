using ErrorOr;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Enums;

namespace VideGreniers.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CategoryType Type { get; private set; }
    public string IconName { get; private set; } = string.Empty;
    public string ColorHex { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    // Navigation properties
    private readonly List<Event> _events = [];
    public IReadOnlyList<Event> Events => _events.AsReadOnly();

    // Private constructor for EF Core
    private Category() { }

    private Category(
        string name,
        string description,
        CategoryType type,
        string iconName,
        string colorHex,
        int sortOrder = 0)
    {
        Name = name;
        Description = description;
        Type = type;
        IconName = iconName;
        ColorHex = colorHex;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public static ErrorOr<Category> Create(
        string name,
        string description,
        CategoryType type,
        string iconName,
        string colorHex,
        int sortOrder = 0)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            errors.Add(Error.Validation("Category.InvalidName", "Category name must be at least 2 characters long."));
        }

        if (name?.Length > 100)
        {
            errors.Add(Error.Validation("Category.NameTooLong", "Category name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add(Error.Validation("Category.InvalidDescription", "Category description is required."));
        }

        if (description?.Length > 500)
        {
            errors.Add(Error.Validation("Category.DescriptionTooLong", "Category description cannot exceed 500 characters."));
        }

        if (string.IsNullOrWhiteSpace(iconName))
        {
            errors.Add(Error.Validation("Category.InvalidIconName", "Icon name is required."));
        }

        if (iconName?.Length > 50)
        {
            errors.Add(Error.Validation("Category.IconNameTooLong", "Icon name cannot exceed 50 characters."));
        }

        if (string.IsNullOrWhiteSpace(colorHex) || !IsValidHexColor(colorHex))
        {
            errors.Add(Error.Validation("Category.InvalidColorHex", "Valid hex color code is required (e.g., #FF5722)."));
        }

        if (sortOrder < 0)
        {
            errors.Add(Error.Validation("Category.InvalidSortOrder", "Sort order cannot be negative."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Category(
            name!.Trim(),
            description!.Trim(),
            type,
            iconName!.Trim(),
            colorHex!.ToUpperInvariant(),
            sortOrder);
    }

    public ErrorOr<Updated> Update(
        string name,
        string description,
        string iconName,
        string colorHex,
        int sortOrder)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            errors.Add(Error.Validation("Category.InvalidName", "Category name must be at least 2 characters long."));
        }

        if (name?.Length > 100)
        {
            errors.Add(Error.Validation("Category.NameTooLong", "Category name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add(Error.Validation("Category.InvalidDescription", "Category description is required."));
        }

        if (description?.Length > 500)
        {
            errors.Add(Error.Validation("Category.DescriptionTooLong", "Category description cannot exceed 500 characters."));
        }

        if (string.IsNullOrWhiteSpace(iconName))
        {
            errors.Add(Error.Validation("Category.InvalidIconName", "Icon name is required."));
        }

        if (iconName?.Length > 50)
        {
            errors.Add(Error.Validation("Category.IconNameTooLong", "Icon name cannot exceed 50 characters."));
        }

        if (string.IsNullOrWhiteSpace(colorHex) || !IsValidHexColor(colorHex))
        {
            errors.Add(Error.Validation("Category.InvalidColorHex", "Valid hex color code is required (e.g., #FF5722)."));
        }

        if (sortOrder < 0)
        {
            errors.Add(Error.Validation("Category.InvalidSortOrder", "Sort order cannot be negative."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name!.Trim();
        Description = description!.Trim();
        IconName = iconName!.Trim();
        ColorHex = colorHex!.ToUpperInvariant();
        SortOrder = sortOrder;
        MarkAsModified();

        return Result.Updated;
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkAsModified();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsModified();
    }

    private static bool IsValidHexColor(string colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return false;
        }

        if (!colorHex.StartsWith('#'))
        {
            return false;
        }

        if (colorHex.Length != 7)
        {
            return false;
        }

        return colorHex[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }

    // Method to be called by Event entity
    internal void AddEvent(Event @event)
    {
        if (!_events.Contains(@event))
        {
            _events.Add(@event);
        }
    }

    internal void RemoveEvent(Event @event)
    {
        _events.Remove(@event);
    }
}