using IronExchange.Basket.API.Grpc;
using IronExchange.WebApp.Services.OrderStatus.IntegrationEvents;
using IronExchange.WebAppComponents.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.JsonWebTokens;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddAuthenticationServices();

        builder.AddRabbitMqEventBus("EventBus")
               .AddEventBusSubscriptions();

        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // Application services
        builder.Services.AddScoped<BasketState>();
        builder.Services.AddScoped<LogOutService>();
        builder.Services.AddSingleton<BasketService>();
        builder.Services.AddSingleton<OrderStatusNotificationService>();
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();
        builder.AddAIServices();

        // HTTP and GRPC client registrations
        builder.Services.AddGrpcClient<Basket.BasketClient>(o => o.Address = new("http://basket-api"))
            .AddAuthToken();

        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("https+http://catalog-api"))
            .AddApiVersion(2.0)
            .AddAuthToken();

        builder.Services.AddHttpClient<OrderingService>(o => o.BaseAddress = new("https+http://ordering-api"))
            .AddApiVersion(1.0)
            .AddAuthToken();
    }

    public static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        eventBus.AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToShippedIntegrationEvent, OrderStatusChangedToShippedIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToCancelledIntegrationEvent, OrderStatusChangedToCancelledIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToSubmittedIntegrationEvent, OrderStatusChangedToSubmittedIntegrationEventHandler>();
    }

    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = config["IdentityUrl"]!;
        var callbackUrl = config["CallBackUrl"]!;
        var sessionLength = config.GetValue("SessionCookieLifetimeMinutes", 60);

        services.AddAuthorization();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionLength);
        })
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            options.Authority = identityUrl;
            options.ClientId = "webapp";
            options.ClientSecret = "secret";

            options.ResponseType = "code";
            options.UsePkce = true;

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.RequireHttpsMetadata = false;
            options.SignedOutRedirectUri = callbackUrl;

            // ---- Scopes (کاملاً هماهنگ با Config) ----
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("phone");
            options.Scope.Add("address");

            options.Scope.Add("roles");
            options.Scope.Add("permissions");

            options.Scope.Add("basket");
            options.Scope.Add("ordering");
            options.Scope.Add("catalog");
            options.Scope.Add("webhooks");
            options.Scope.Add("webshoppingagg");

            options.Scope.Add("api.roles");
            options.Scope.Add("api.permissions");

            options.Scope.Add("offline_access");
        });

        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }
    private static void AddAIServices(this IHostApplicationBuilder builder)
    {
        ChatClientBuilder? chatClientBuilder = null;
        if (builder.Configuration["OllamaEnabled"] is string ollamaEnabled && bool.Parse(ollamaEnabled))
        {
            chatClientBuilder = builder.AddOllamaApiClient("chat")
                .AddChatClient();
        }
        else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("chatModel")))
        {
            chatClientBuilder = builder.AddOpenAIClientFromConfiguration("chatModel")
                .AddChatClient();
        }

        chatClientBuilder?.UseFunctionInvocation();
    }

    public static async Task<string?> GetBuyerIdAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("sub")?.Value;
    }

    public static async Task<string?> GetUserNameAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("name")?.Value;
    }
}
