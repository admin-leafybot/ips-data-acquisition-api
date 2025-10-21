using Microsoft.AspNetCore.Identity;

namespace IPSDataAcquisition.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<ButtonPress> ButtonPresses { get; set; } = new List<ButtonPress>();
    public virtual ICollection<IMUData> IMUDataRecords { get; set; } = new List<IMUData>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

