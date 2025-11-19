namespace IronExchange.Identity.API.Configuration;

public static class ExternalAuthenticationExtensions
{
    public static IServiceCollection AddExternalAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication();

        // Google Authentication
        if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
        {
            authBuilder.AddGoogle("Google", options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;

                options.ClaimActions.MapJsonKey("picture", "picture", "url");
                options.ClaimActions.MapJsonKey("locale", "locale", "string");

                options.Events.OnCreatingTicket = context =>
                {
                    var picture = context.User.GetProperty("picture").GetString();
                    if (!string.IsNullOrEmpty(picture))
                    {
                        context.Identity.AddClaim(new Claim("picture", picture));
                    }
                    return Task.CompletedTask;
                };
            });
        }

        // Microsoft Authentication
        if (!string.IsNullOrEmpty(configuration["Authentication:Microsoft:ClientId"]))
        {
            authBuilder.AddMicrosoftAccount("Microsoft", options =>
            {
                options.ClientId = configuration["Authentication:Microsoft:ClientId"];
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                options.CallbackPath = "/signin-microsoft";
                options.SaveTokens = true;
            });
        }

        // Facebook Authentication
        if (!string.IsNullOrEmpty(configuration["Authentication:Facebook:AppId"]))
        {
            authBuilder.AddFacebook("Facebook", options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/signin-facebook";
                options.SaveTokens = true;

                options.Scope.Add("email");
                options.Scope.Add("public_profile");

                options.Fields.Add("name");
                options.Fields.Add("email");
                options.Fields.Add("picture");
            });
        }

        return services;
    }
}
