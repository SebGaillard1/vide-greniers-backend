namespace VideGreniers.Domain.Enums;

/// <summary>
/// Represents different types of second-hand sales events
/// </summary>
public enum EventType
{
    /// <summary>
    /// Individual garage sale at private residence
    /// </summary>
    GarageSale = 0,

    /// <summary>
    /// Large organized flea market with multiple vendors
    /// </summary>
    FleaMarket = 1,

    /// <summary>
    /// Estate sale typically after someone passes away
    /// </summary>
    EstateSale = 2,

    /// <summary>
    /// Organized by church or community group
    /// </summary>
    ChurchSale = 3,

    /// <summary>
    /// School fundraising event
    /// </summary>
    SchoolSale = 4,

    /// <summary>
    /// Moving sale when relocating
    /// </summary>
    MovingSale = 5,

    /// <summary>
    /// Community-wide garage sale event
    /// </summary>
    CommunityWide = 6,

    /// <summary>
    /// Specialized sale (books, clothes, etc.)
    /// </summary>
    Specialized = 7,

    /// <summary>
    /// Other type of second-hand sale
    /// </summary>
    Other = 99
}