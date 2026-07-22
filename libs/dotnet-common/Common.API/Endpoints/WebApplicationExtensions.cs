using System.Reflection;

namespace Common.API.Endpoints;

public static class WebApplicationExtensions
{
    /// <summary>
    ///     Discovers all <see cref="IEndpointGroup" /> implementations in <paramref name="assembly" />
    ///     and registers each as a route group with a matching OpenAPI tag. The route prefix defaults
    ///     to <c>/api/{ClassName}</c> but can be overridden via <see cref="IEndpointGroup.RoutePrefix" />.
    /// </summary>
    public static WebApplication MapEndpoints(this WebApplication app, Assembly assembly)
    {
        IEnumerable<Type> endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.IsAssignableTo(typeof(IEndpointGroup)));

        foreach (Type type in endpointGroupTypes)
        {
            string groupName = type.Name;
            string routePrefix = type.GetProperty(nameof(IEndpointGroup.RoutePrefix))
                    ?.GetValue(null) as string ?? $"/api/{groupName}";
            RouteGroupBuilder group = app.MapGroup(routePrefix).WithTags(groupName);
            type.GetMethod(nameof(IEndpointGroup.Map))!.Invoke(null, [group]);
        }

        return app;
    }
}
