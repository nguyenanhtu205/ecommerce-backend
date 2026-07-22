using Common.API.ExceptionHandling;
using Common.API.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Common.API;

public static class CommonApiExtensions
{
    public static IServiceCollection AddCommonApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
        });

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        return services;
    }
}
