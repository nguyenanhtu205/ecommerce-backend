using MongoDB.Bson.Serialization.Conventions;

namespace ProductCatalogService.Infrastructure.Data;

public static class MongoConventions
{
    private static bool s_registered;

    public static void Register()
    {
        if (s_registered)
        {
            return;
        }

        ConventionPack pack = [new CamelCaseElementNameConvention()];
        ConventionRegistry.Register("camelCase", pack, _ => true);

        s_registered = true;
    }
}
