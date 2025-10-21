namespace IPSDataAcquisition.Domain.Entities;

public class Session
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public long StartTimestamp { get; set; }
    public long? EndTimestamp { get; set; }
    public bool IsSynced { get; set; } = true;
    public string Status { get; set; } = SessionStatus.InProgress;
    public string PaymentStatus { get; set; } = PaymentStatusEnum.Unpaid;
    public string? Remarks { get; set; }
    public decimal? BonusAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<ButtonPress> ButtonPresses { get; set; } = new List<ButtonPress>();
    public virtual ICollection<IMUData> IMUDataPoints { get; set; } = new List<IMUData>();
}

public static class SessionStatus
{
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

public static class PaymentStatusEnum
{
    public const string Unpaid = "unpaid";
    public const string Paid = "paid";
}

