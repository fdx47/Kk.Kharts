using System.Security.Cryptography;
using System.Text;

namespace Kk.Kharts.Api.Middlewares;

/// <summary>
/// Middleware de protection du Swagger UI par authentification cookie.
/// Affiche une page de login modale avant d'autoriser l'accès à /swagger.
/// Les credentials sont validées contre la section SwaggerAuth de appsettings.json.
/// Après authentification, l'utilisateur doit encore fournir un token JWT pour utiliser les endpoints.
/// </summary>
public sealed class SwaggerAuthMiddleware
{
    private const string CookieName = "SwaggerAuthTicket";
    private const string LoginPath = "/swagger/login";
    private const string LogoutPath = "/swagger/logout";

    private readonly RequestDelegate _next;
    private readonly string _expectedUser;
    private readonly string _expectedPassword;
    private readonly string _ticket;

    public SwaggerAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _expectedUser = configuration["SwaggerAuth:Username"] ?? string.Empty;
        _expectedPassword = configuration["SwaggerAuth:Password"] ?? string.Empty;
        _ticket = BuildTicket(_expectedUser, _expectedPassword);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var isLogin = context.Request.Path.Equals(LoginPath, StringComparison.OrdinalIgnoreCase);
        var isLogout = context.Request.Path.Equals(LogoutPath, StringComparison.OrdinalIgnoreCase);

        // ── Logout ──────────────────────────────────────────────
        if (isLogout)
        {
            context.Response.Cookies.Delete(CookieName);
            context.Response.Redirect(LoginPath);
            return;
        }

        // ── POST /swagger/login ─────────────────────────────────
        if (isLogin && HttpMethods.IsPost(context.Request.Method))
        {
            await HandleLoginPostAsync(context);
            return;
        }

        // ── Cookie válido → prosseguir ──────────────────────────
        if (HasValidCookie(context))
        {
            if (isLogin)
            {
                context.Response.Redirect("/swagger");
                return;
            }

            await _next(context);
            return;
        }

        // ── Sem cookie → mostrar página de login ────────────────
        var redirect = isLogin ? null : context.Request.Path + context.Request.QueryString;
        await RenderLoginPageAsync(context, redirect);
    }

    // ────────────────────────────────────────────────────────────
    // Métodos privados
    // ────────────────────────────────────────────────────────────

    private bool HasValidCookie(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_ticket))
            return false;

        return context.Request.Cookies.TryGetValue(CookieName, out var value)
               && string.Equals(value, _ticket, StringComparison.Ordinal);
    }

    private async Task HandleLoginPostAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_expectedUser) || string.IsNullOrWhiteSpace(_expectedPassword))
        {
            await RenderLoginPageAsync(context, errorMessage: "SwaggerAuth n’est pas encore configuré dans le fichier appsettings.");
            return;
        }

        var form = await context.Request.ReadFormAsync();
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var redirect = form["redirect"].ToString();

        if (string.Equals(username, _expectedUser, StringComparison.Ordinal)
            && string.Equals(password, _expectedPassword, StringComparison.Ordinal))
        {
            var target = IsSwaggerPath(redirect) ? redirect : "/swagger";

            context.Response.Cookies.Append(CookieName, _ticket, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            context.Response.Redirect(target);
            return;
        }

        await RenderLoginPageAsync(context, redirect, "Identifiants incorrects. Veuillez réessayer.");
    }

    private static async Task RenderLoginPageAsync(HttpContext context, string? redirect = null, string? errorMessage = null)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";

        var safeRedirect = IsSwaggerPath(redirect) ? redirect : "/swagger";

        var errorHtml = string.IsNullOrWhiteSpace(errorMessage)
            ? string.Empty
            : $@"<div class=""error"">{System.Net.WebUtility.HtmlEncode(errorMessage)}</div>";

        var html = LoginPageTemplate
            .Replace("{{ERROR}}", errorHtml)
            .Replace("{{REDIRECT}}", System.Net.WebUtility.HtmlEncode(safeRedirect));

        await context.Response.WriteAsync(html);
    }

    private static bool IsSwaggerPath(string? path)
        => !string.IsNullOrWhiteSpace(path) && path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);

    private static string BuildTicket(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return string.Empty;

        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{username}:{password}")));
    }

    // ────────────────────────────────────────────────────────────
    // Template HTML da página de login
    // ────────────────────────────────────────────────────────────


    private const string LoginPageTemplate = @"<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>Swagger – Authentification</title>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: radial-gradient(ellipse at top, #1e293b 0%, #0f172a 60%);
            color: #e2e8f0;
        }
        .card {
            width: min(92vw, 400px);
            background: #f8fafc;
            border-radius: 16px;
            padding: 36px 32px 28px;
            box-shadow: 0 25px 60px rgba(0,0,0,.4);
            color: #1e293b;
        }
        .card h1 {
            font-size: 1.25rem;
            margin-bottom: 6px;
        }
        .card p.sub {
            font-size: .82rem;
            color: #64748b;
            margin-bottom: 22px;
        }
        label {
            display: block;
            font-size: .78rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: .06em;
            color: #475569;
            margin-bottom: 5px;
        }
        input[type=text], input[type=password] {
            width: 100%;
            padding: 10px 12px;
            margin-bottom: 14px;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: .95rem;
            background: #fff;
            transition: border-color .2s;
        }
        input:focus {
            outline: none;
            border-color: #3b82f6;
            box-shadow: 0 0 0 3px rgba(59,130,246,.15);
        }
        button {
            width: 100%;
            padding: 11px;
            border: none;
            border-radius: 8px;
            background: linear-gradient(135deg, #2563eb, #38bdf8);
            color: #fff;
            font-weight: 600;
            font-size: .95rem;
            cursor: pointer;
            margin-top: 4px;
            transition: transform .15s, box-shadow .15s;
        }
        button:hover {
            transform: translateY(-1px);
            box-shadow: 0 8px 20px rgba(37,99,235,.35);
        }
        .error {
            background: #fef2f2;
            color: #b91c1c;
            border: 1px solid #fecaca;
            padding: 9px 12px;
            border-radius: 8px;
            font-size: .85rem;
            margin-bottom: 14px;
        }
        .footer {
            text-align: center;
            margin-top: 18px;
            font-size: .75rem;
            color: #94a3b8;
        }
    </style>
</head>
<body>
    <div class=""card"">
        <h1>🔒 Kropkontrol</h1>
        <p class=""sub"">Veuillez saisir les identifiants, pour accéder à la documentation de l’API.</p>
        <form method=""post"" action=""/swagger/login"">
            {{ERROR}}
            <label for=""username"">Nom d’utilisateur</label>
            <input id=""username"" name=""username"" type=""text"" autocomplete=""username"" required autofocus />
            <label for=""password"">Mot de passe</label>
            <input id=""password"" name=""password"" type=""password"" autocomplete=""current-password"" required />
            <input type=""hidden"" name=""redirect"" value=""{{REDIRECT}}"" />
            <button type=""submit"">Se connecter</button>
        </form>
        <div class=""footer"">KropKontrol &copy; 2025 - 2026</div>
    </div>
</body>
</html>";
}
