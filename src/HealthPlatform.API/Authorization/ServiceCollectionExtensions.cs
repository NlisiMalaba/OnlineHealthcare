using HealthPlatform.Application.Security;
using Microsoft.AspNetCore.Authorization;

namespace HealthPlatform.API.Authorization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthPlatformAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Patient, p => p.RequireRole(ApplicationRoles.Patient));
            options.AddPolicy(AuthorizationPolicies.Doctor, p => p.RequireRole(ApplicationRoles.Doctor));
            options.AddPolicy(AuthorizationPolicies.Pharmacy, p => p.RequireRole(ApplicationRoles.Pharmacy));
            options.AddPolicy(AuthorizationPolicies.LabPartner, p => p.RequireRole(ApplicationRoles.LabPartner));
            options.AddPolicy(AuthorizationPolicies.Insurer, p => p.RequireRole(ApplicationRoles.Insurer));
            options.AddPolicy(AuthorizationPolicies.Admin, p => p.RequireRole(ApplicationRoles.Admin));
        });

        return services;
    }
}
