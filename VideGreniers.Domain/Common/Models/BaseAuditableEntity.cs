namespace VideGreniers.Domain.Common.Models;

public abstract class BaseAuditableEntity : BaseEntity
{
    public Guid? CreatedByUserId { get; private set; }

    public Guid? ModifiedByUserId { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedOnUtc { get; private set; }

    public Guid? DeletedByUserId { get; private set; }

    protected void SetCreatedBy(Guid userId)
    {
        CreatedByUserId = userId;
    }

    protected void SetModifiedBy(Guid userId)
    {
        ModifiedByUserId = userId;
        MarkAsModified();
    }

    public void SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
        DeletedByUserId = deletedBy;
        MarkAsModified();
    }

    public void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedByUserId = null;
        MarkAsModified();
    }
}