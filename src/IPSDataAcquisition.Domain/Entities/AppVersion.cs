using IPSDataAcquisition.Domain.Common;

namespace IPSDataAcquisition.Domain.Entities;

public class AppVersion : BaseEntity
{
    public string VersionName { get; set; } = string.Empty;
    public bool Active { get; set; } = false;
}

