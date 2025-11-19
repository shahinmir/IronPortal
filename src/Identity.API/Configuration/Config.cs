using Duende.IdentityServer.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace IronExchange.Identity.API.Configuration;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
            new IdentityResource("roles", "User roles", new[] { "role" }),
            new IdentityResource("permissions", "User permissions", new[] { "permission" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("basket", "Basket API"),
            new ApiScope("catalog", "Catalog API"),
            new ApiScope("ordering", "Ordering API"),
            new ApiScope("webhooks", "Webhooks API"),
            new ApiScope("mobilebff", "Mobile BFF"),
            new ApiScope("webshoppingagg", "Web Shopping Aggregator"),

            new ApiScope("api.roles", "API Roles"),
            new ApiScope("api.permissions", "API Permissions")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("basket")           { Scopes = { "basket" },          UserClaims = { "role", "permission" } },
            new ApiResource("catalog")          { Scopes = { "catalog" },         UserClaims = { "role", "permission" } },
            new ApiResource("ordering")         { Scopes = { "ordering" },        UserClaims = { "role", "permission" } },
            new ApiResource("webhooks")         { Scopes = { "webhooks" },        UserClaims = { "role", "permission" } },
            new ApiResource("mobilebff")        { Scopes = { "mobilebff" },       UserClaims = { "role", "permission" } },
            new ApiResource("webshoppingagg")   { Scopes = { "webshoppingagg" },  UserClaims = { "role", "permission" } }
        };

    public static IEnumerable<Client> Clients(IConfiguration configuration) =>
        new List<Client>
        {
            // ------------ Aspire Dashboard -----------------
            new Client
            {
                ClientId = "aspire-dashboard",
                ClientName = "Aspire Dashboard",

                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RequirePushedAuthorization = false,

                RedirectUris =
                {
                    "https://localhost:7298/signin-oidc",
                    "https://localhost:7298/authentication/login-callback"
                },
                PostLogoutRedirectUris =
                {
                    "https://localhost:7298/signout-callback-oidc",
                    "https://localhost:7298/authentication/logout-callback"
                },

                AllowedScopes =
                {
                    "openid", "profile", "email", "phone", "address",
                    "roles", "permissions",
                    "offline_access"
                },
                AllowOfflineAccess = true
            },

            // ------------ WebApp (Blazor Server) -----------------
            new Client
            {
                ClientId = "webapp",
                ClientName = "Web Application",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,

                RedirectUris =
                {
                    $"{configuration["WebAppClient"]}/signin-oidc"
                },
                PostLogoutRedirectUris =
                {
                    $"{configuration["WebAppClient"]}/signout-callback-oidc"
                },

                AllowedScopes =
                {
                    // Identity
                    "openid", "profile", "email", "phone", "address",
                    "roles", "permissions",

                    // API's
                    "basket",
                    "ordering",
                    "catalog",
                    "webhooks",
                    "webshoppingagg",

                    "api.roles",
                    "api.permissions",

                    "offline_access"
                }
            },

            // ------------ Backend Microservices -----------------
            new Client
            {
                ClientId = "basket",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "basket", "catalog" }
            },

            new Client
            {
                ClientId = "ordering",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "ordering", "basket", "catalog" }
            },

            new Client
            {
                ClientId = "webhooks",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "webhooks" }
            },

            new Client
            {
                ClientId = "mobilebff",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "http://localhost:5200/signin-oidc" },
                RequirePkce = true,
                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    "openid", "profile", "email",
                    "mobilebff", "basket", "catalog", "ordering",
                    "offline_access"
                }
            }
        };
}
