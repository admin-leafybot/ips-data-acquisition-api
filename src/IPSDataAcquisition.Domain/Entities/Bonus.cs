using IPSDataAcquisition.Domain.Common;

namespace IPSDataAcquisition.Domain.Entities;

public class Bonus : BaseEntity
{
    public DateTime Date { get; set; }
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public int SessionsCompleted { get; set; }
    public string? Description { get; set; }
}

