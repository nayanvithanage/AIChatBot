namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// User roles in the DMS system
    /// </summary>
    public enum UserRole
    {
        Admin = 1,
        ProjectManager = 2,
        ProjectUser = 3
    }

    /// <summary>
    /// Document workflow status
    /// </summary>
    public enum DocumentStatus
    {
        Draft = 1,
        InReview = 2,
        IFC = 3, // Issued for Construction
        Approved = 4,
        Superseded = 5,
        Archived = 6
    }

    /// <summary>
    /// Document approval status
    /// </summary>
    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        RevisionRequired = 4
    }

    /// <summary>
    /// Document revision status
    /// </summary>
    public enum RevisionStatus
    {
        Draft = 1,
        UnderReview = 2,
        Released = 3
    }

    /// <summary>
    /// Document link types
    /// </summary>
    public enum LinkType
    {
        Reference = 1,
        Supersedes = 2,
        Related = 3
    }

    /// <summary>
    /// Linked item types (RFI, Transmittal, etc.)
    /// </summary>
    public enum LinkedItemType
    {
        RFI = 1,
        Transmittal = 2,
        Mail = 3,
        Form = 4,
        Task = 5
    }

    /// <summary>
    /// Linked item status
    /// </summary>
    public enum ItemStatus
    {
        Open = 1,
        InProgress = 2,
        Closed = 3,
        Pending = 4
    }

    /// <summary>
    /// Document action types
    /// </summary>
    public enum ActionType
    {
        Created = 1,
        Updated = 2,
        Reviewed = 3,
        Approved = 4,
        Rejected = 5,
        ViewFileReplaced = 6,
        ViewFileRemoved = 7,
        BluebeamReview = 8,
        DetailsChanged = 9,
        StatusChanged = 10
    }
}
