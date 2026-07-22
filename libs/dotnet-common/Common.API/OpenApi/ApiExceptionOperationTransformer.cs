using Microsoft.AspNetCore.Authorization;

namespace Common.API.OpenApi;

/// <summary>
///     Adds standard error responses to every OpenAPI operation. A 400 Bad Request is added to all
///     operations because every request passes through <c>ValidationBehaviour</c> in the MediatR
///     pipeline. 401 Unauthorized and 403 Forbidden are added only to operations that carry
///     <see cref="IAuthorizeData" /> metadata.
/// </summary>
internal sealed class ApiExceptionOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Responses ??= [];
        operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
        operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });

        bool requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAuthorizeData);

        if (!requiresAuth)
        {
            return Task.CompletedTask;
        }

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        operation.Security ??= [];
        OpenApiSecurityRequirement securityRequirement = new();
        OpenApiSecuritySchemeReference schemeReference = new("Bearer", context.Document);
        securityRequirement.Add(schemeReference, []);
        operation.Security.Add(securityRequirement);

        return Task.CompletedTask;
    }
}
