using IPSDataAcquisition.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IPSDataAcquisition.Infrastructure.Services;

public class AdminSecurityService : IAdminSecurityService
{
    private readonly IConfiguration _configuration;

    public AdminSecurityService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidateChangeVerificationSecurityKey(string securityKey)
    {
        var expectedKey = _configuration["AdminSettings:ChangeVerificationSecurityKey"];
        return !string.IsNullOrEmpty(expectedKey) && securityKey == expectedKey;
    }
}

