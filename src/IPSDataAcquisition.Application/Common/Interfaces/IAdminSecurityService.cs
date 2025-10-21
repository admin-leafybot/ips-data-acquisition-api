namespace IPSDataAcquisition.Application.Common.Interfaces;

public interface IAdminSecurityService
{
    bool ValidateChangeVerificationSecurityKey(string securityKey);
}

