using Grpc.Core;
using Grpc.Core.Interceptors;
using IronExchange.Authorization.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace IronExchange.Authorization.Grpc;

public class PermissionAuthorizationInterceptor : Interceptor
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationInterceptor(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return await continuation(request, context);

        var endpoint = httpContext.GetEndpoint();
        var permissionAttributes = endpoint?.Metadata.GetOrderedMetadata<RequirePermissionAttribute>() ?? [];

        foreach (var attr in permissionAttributes)
        {
            var permissions = attr.GetPermissions();
            var policy = permissions.ToPolicyString();

            var result = await _authorizationService.AuthorizeAsync(httpContext.User, null, policy);
            if (!result.Succeeded)
                throw new RpcException(new Status(StatusCode.PermissionDenied,
                    $"Missing permission(s): {string.Join(",", permissions)}"));
        }

        return await continuation(request, context);
    }
}
