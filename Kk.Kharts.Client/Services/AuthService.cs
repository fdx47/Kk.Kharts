using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Kk.Kharts.Client.Services
{
    public class AuthService(AuthenticationStateProvider authenticationStateProvider)
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;

        public async Task<string?> GetUserNameAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst("email")?.Value;
        }

        public async Task<string?> GetUserRoleAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.Identity!.IsAuthenticated;
        }

        public async Task<string?> GetNameAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst("nom")?.Value;
        }
    }
}