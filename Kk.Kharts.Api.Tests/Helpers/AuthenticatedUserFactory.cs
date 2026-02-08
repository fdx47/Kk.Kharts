using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Factory pour créer des utilisateurs authentifiés avec différents rôles pour les tests.
/// Permet de tester les autorisations de manière cohérente.
/// </summary>
public static class AuthenticatedUserFactory
{
    public static AuthenticatedUserDto CreateRootUser(int companyId = 1) => new()
    {
        UserId = 1,
        Role = "Root",
        CompanyId = companyId,
        AccessLevel = UserAccessLevel.CompanyAndSubsidiaries
    };

    public static AuthenticatedUserDto CreateAdminUser(int companyId = 1) => new()
    {
        UserId = 2,
        Role = "Admin",
        CompanyId = companyId,
        AccessLevel = UserAccessLevel.CompanyAndSubsidiaries
    };

    public static AuthenticatedUserDto CreateReadOnlyUser(int companyId = 1) => new()
    {
        UserId = 3,
        Role = "ReadOnly",
        CompanyId = companyId,
        AccessLevel = UserAccessLevel.Technician
    };

    public static AuthenticatedUserDto CreateTechnicianUser(int companyId = 1) => new()
    {
        UserId = 4,
        Role = "Technician",
        CompanyId = companyId,
        AccessLevel = UserAccessLevel.Technician
    };

    /// <summary>
    /// Crée un utilisateur avec préférences de notification spécifiques.
    /// Email: cesar@blazor.com comme demandé.
    /// </summary>
    public static AuthenticatedUserDto CreateNotificationTestUser(
        NotificationChannelPreference preference = NotificationChannelPreference.Tous,
        int companyId = 1) => new()
    {
        UserId = 42,
        Role = "Root",
        CompanyId = companyId,
        AccessLevel = UserAccessLevel.CompanyAndSubsidiaries
    };
}
