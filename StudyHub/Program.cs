using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyHub.Data;
using StudyHub.Services; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StudyHubContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var clientSecret = builder.Configuration["Keycloak:ClientSecret"];

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:8080/realms/studyhub";
        options.ClientId = "studyhub-web";
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
        options.CallbackPath = "/signin-oidc";
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity == null) return Task.CompletedTask;
                
                var accessToken = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                var realmAccess = accessToken?.Claims
                    .FirstOrDefault(c => c.Type == "realm_access")?.Value;

                if (realmAccess != null)
                {
                    var json = System.Text.Json.JsonDocument.Parse(realmAccess);
                    if (json.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            claimsIdentity.AddClaim(
                                new Claim(ClaimTypes.Role, role.GetString() ?? "")
                            );
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "StudyHub_";
});


builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<RedisService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
