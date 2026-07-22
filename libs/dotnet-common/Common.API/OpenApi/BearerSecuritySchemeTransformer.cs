using Microsoft.AspNetCore.Authentication;

namespace Common.API.OpenApi;

/// <summary>
///     Adds the Bearer JWT security scheme to the OpenAPI document when Bearer authentication
///     is configured, enabling the Scalar UI to send <c>Authorization: Bearer &lt;token&gt;</c>
///     headers from the interactive documentation.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        IEnumerable<AuthenticationScheme> authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            Dictionary<string, IOpenApiSecurityScheme> requirements = new()
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}
