using System.Security.Claims;
using BlazorSocial.Infrastructure;
using Yarp.ReverseProxy.Forwarder;

namespace BlazorSocial.Proxy;

public sealed class AddUserIdTransformer : HttpTransformer
{
    public override async ValueTask TransformRequestAsync(
        HttpContext httpContext,
        HttpRequestMessage proxyRequest,
        string destinationPrefix,
        CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is not null)
            proxyRequest.Headers.TryAddWithoutValidation(InternalHeaders.UserId, userId);
    }
}
