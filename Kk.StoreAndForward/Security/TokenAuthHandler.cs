using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace KK.UG6x.StoreAndForward.Security;

public class TokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TokenAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

        var token = authHeader.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            if (parts.Length == 2 && parts[0] == "admin")
            {
                var claims = new[] { new Claim(ClaimTypes.Name, parts[0]) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
        catch { }

        return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
    }
}
